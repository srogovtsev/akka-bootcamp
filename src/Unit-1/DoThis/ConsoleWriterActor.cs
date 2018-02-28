using System;
using Akka.Actor;

namespace WinTail
{
    /// <summary>
    /// Actor responsible for serializing message writes to the console.
    /// (write one message at a time, champ :)
    /// </summary>
    class ConsoleWriterActor : UntypedActor
    {
        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case Messages.InputError msg:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(msg.Reason);
                    break;
                case Messages.InputSuccess msg:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(msg.Reason);
                    break;
                default:
                    Console.WriteLine(message);
                    break;
            }

            Console.ResetColor();
        }
    }
}
