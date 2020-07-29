using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;
using System.Composition.Hosting;
using Oqtane.ChatHubs.Hubs;
using Oqtane.ChatHubs.Services;
using Oqtane.ChatHubs.Repository;
using Oqtane.Modules;

namespace Oqtane.ChatHubs.Commands
{
    public class CommandManager
    {
        private readonly int _userId;
        private readonly string _connectionId;
        private readonly int _roomId;
        private readonly ChatHub _chatHub;
        private readonly IChatHubService _chatService;
        private readonly IChatHubRepository _repository;
        private readonly UserManager<IdentityUser> _userManager;

        private static Dictionary<string, ICommand> _commandCache;
        private static readonly Lazy<IList<ICommand>> _commands = new Lazy<IList<ICommand>>(GetCommands);

        public CommandManager(int userId, 
                              string connectionId,
                              int roomId,
                              ChatHub chatHub,
                              IChatHubService service,
                              IChatHubRepository repository,
                              UserManager<IdentityUser> userManager)
        {
            _userId = userId;
            _connectionId = connectionId;
            _roomId = roomId;
            _chatHub = chatHub;
            _chatService = service;
            _repository = repository;
            _userManager = userManager;
        }

        public string ParseCommand(string commandString, out string[] args)
        {
            var parts = commandString.Substring(1).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
            {
                args = new string[0];
                return null;
            }

            args = parts.Skip(1).ToArray();
            return parts[0];
        }
        public async Task<bool> TryHandleCommand(string command)
        {
            command = command.Trim();
            if (!Regex.IsMatch(command, @"^\/[A-Za-z0-9?]+?"))
            {
                return false;
            }

            string[] args;
            var commandName = ParseCommand(command, out args);
            return await TryHandleCommand(commandName, args);
        }

        public async Task<bool> TryHandleCommand(string commandName, string[] args)
        {
            if (String.IsNullOrEmpty(commandName))
            {
                return false;
            }

            commandName = commandName.Trim();
            if (commandName.StartsWith("/"))
            {
                return false;
            }

            var context = new CommandServicesContext
            {
                ChatHub = _chatHub,
                ChatHubService = _chatService,
                ChatHubRepository = _repository,                
                UserManager = _userManager
            };

            var callerContext = new CommandCallerContext
            {
                ConnectionId = _connectionId,
                UserId = _userId,
                RoomId = _roomId
            };

            ICommand command;
            try
            {
                command = MatchCommand(commandName);
                await command.Execute(context, callerContext, args);
            }
            catch (CommandNotFoundException e)
            {
                throw new Exception(e.Message);
            }
            catch (CommandAmbiguityException e)
            {
                throw new Exception(e.Message);
            }

            return true;
        }

        public ICommand MatchCommand(string commandName)
        {
            ICommand command = null;
            if (_commandCache == null)
            {
                var commands = _commands.Value.Where(x => x.GetType().GetCustomAttributes(true).OfType<CommandAttribute>().FirstOrDefault() != null)
                                        .Select(y => new { Names = y.GetType().GetCustomAttributes(true).OfType<CommandAttribute>().FirstOrDefault().Names, Command = y });

                _commandCache = commands.ToDictionary(c => string.Join('|', c.Names), c => c.Command, StringComparer.OrdinalIgnoreCase);
            }

            IList<string> candidates = null;
            foreach (string key in _commandCache.Keys)
            {
                string[] commandNames = key.Split('|');
                var exactMatches = commandNames.Where(x => x.Equals(commandName, StringComparison.OrdinalIgnoreCase)).ToList();

                if (exactMatches.Count == 1)
                {
                    candidates = exactMatches;
                }

                switch (candidates.Count)
                {
                    case 1:
                        _commandCache.TryGetValue(key, out command);
                        commandName = candidates[0];
                        break;
                    case 0:
                        throw new CommandNotFoundException();
                    default:
                        throw new CommandAmbiguityException(candidates);
                }
            }

            return command;
        }

        private static IList<ICommand> GetCommands()
        {
            // Use MEF -> System.Compostion nuget package
            var configuration = new ContainerConfiguration().WithAssembly(typeof(CommandManager).Assembly);
            var container = configuration.CreateContainer();
            IList<ICommand> iCommands = container.GetExports<ICommand>("ICommand").ToList();
            return iCommands;
        }        
        public static IEnumerable<CommandMetaData> GetCommandsMetaData()
        {
            var commandsMetaData = _commands.Value.Where(x => x.GetType().GetCustomAttributes(true).OfType<CommandAttribute>().FirstOrDefault() != null)
                                        .Select(y => new CommandMetaData {
                                            Names = y.GetType().GetCustomAttributes(true).OfType<CommandAttribute>().FirstOrDefault().Names,
                                            Arguments = y.GetType().GetCustomAttributes(true).OfType<CommandAttribute>().FirstOrDefault().Arguments,
                                            Roles = y.GetType().GetCustomAttributes(true).OfType<CommandAttribute>().FirstOrDefault().Roles,
                                            Usage = y.GetType().GetCustomAttributes(true).OfType<CommandAttribute>().FirstOrDefault().Usage
                                        });

            return commandsMetaData;
        }
        
    }
}