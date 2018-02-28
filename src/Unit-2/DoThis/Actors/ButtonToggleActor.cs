using System.Windows.Forms;
using Akka.Actor;

namespace ChartApp.Actors
{
    /// <summary>
    /// Actor responsible for managing button toggles
    /// </summary>
    public class ButtonToggleActor : UntypedActor
    {
        #region Message types

        /// <summary>
        /// Toggles this button on or off and sends an appropriate messages
        /// to the <see cref="PerformanceCounterCoordinatorActor"/>
        /// </summary>
        public class Toggle { }

        #endregion

        private readonly CounterType _myCounterType;
        private bool _isToggledOn;
        private readonly Button _myButton;
        private readonly IActorRef _coordinatorActor;

        public ButtonToggleActor(IActorRef coordinatorActor, Button myButton, CounterType myCounterType,  bool isToggledOn = false)
        {
            _coordinatorActor = coordinatorActor;
            _myButton = myButton;
            _isToggledOn = isToggledOn;
            _myCounterType = myCounterType;
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case Toggle _ when _isToggledOn:
                    //stop watching this counter
                    _coordinatorActor.Tell(new PerformanceCounterCoordinatorActor.Unwatch(_myCounterType));

                    FlipToggle();
                    break;
                case Toggle _ when !_isToggledOn:
                    //start watching this counter
                    _coordinatorActor.Tell(new PerformanceCounterCoordinatorActor.Watch(_myCounterType));

                    FlipToggle();
                    break;
                default:
                    Unhandled(message);
                    break;
            }
        }

        private void FlipToggle()
        {
            //flip the toggle
            _isToggledOn = !_isToggledOn;
            
            //change the text of the button
            _myButton.Text = $"{_myCounterType.ToString().ToUpperInvariant()} ({(_isToggledOn ? "ON" : "OFF")})";
        }
    }
}
