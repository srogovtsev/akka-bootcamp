﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Akka.Actor;

namespace ChartApp.Actors
{
    /// <summary>
    /// Actor responsible for monitoring a specific <see cref="PerformanceCounter"/>
    /// </summary>
    public class PerformanceCounterActor : UntypedActor
    {
        private readonly string _seriesName;
        private readonly Func<PerformanceCounter> _performanceCounterGenerator;
        private PerformanceCounter _counter;

        private readonly HashSet<IActorRef> _subscriptions;
        private readonly ICancelable _cancelPublishing;

        public PerformanceCounterActor(string seriesName, Func<PerformanceCounter> performanceCounterGenerator)
        {
            _seriesName = seriesName;
            _performanceCounterGenerator = performanceCounterGenerator;
            _subscriptions = new HashSet<IActorRef>();
            _cancelPublishing = new Cancelable(Context.System.Scheduler);
        }

        #region Actor lifecycle methods

        protected override void PreStart()
        {
            //create a new instance of the performance counter
            _counter = _performanceCounterGenerator();
            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(250), Self,
                new GatherMetrics(), Self, _cancelPublishing);
        }

        protected override void PostStop()
        {
            try
            {
                //terminate the scheduled task
                _cancelPublishing.Cancel(false);
                _counter.Dispose();
            }
            catch 
            {
                //don't care about additional "ObjectDisposed" exceptions
            }
            finally
            {
                base.PostStop();    
            }
        }

        #endregion

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case GatherMetrics _:
                    //publish latest counter value to all subscribers
                    var metric = new Metric(_seriesName, _counter.NextValue());
                    foreach(var sub in _subscriptions)
                        sub.Tell(metric);
                    break;
                case SubscribeCounter sc:
                    // add a subscription for this counter
                    // (it's parent's job to filter by counter types)
                    _subscriptions.Add(sc.Subscriber);
                    break;
                case UnsubscribeCounter uc:
                    // remove a subscription from this counter
                    _subscriptions.Remove(uc.Subscriber);
                    break;
            }
        }
    }
}
