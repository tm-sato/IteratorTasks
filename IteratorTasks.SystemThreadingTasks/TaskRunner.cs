﻿using System;
using IT = IteratorTasks;
using ST = System.Threading;
using TT = System.Threading.Tasks;

namespace IteratorTasks
{
    /// <summary>
    /// スレッドプールで <see cref="IteratorTasks.TaskScheduler.Update"/> を回すクラス。
    /// 互換性用。
    /// 1 Runner 1 Scheduler なんだけど、1 Runner が複数持つように変えたいけど、既存のものはそっとしておきたいので別クラスを作った → <see cref="MultiTaskRunner"/>。
    /// </summary>
    public class TaskRunner
    {
        private TT.Task _task;
        private volatile bool _isAlive;
        private ST.CancellationTokenSource _cts = new ST.CancellationTokenSource();

        public IT.TaskScheduler Scheduler { get; private set; }
        public ST.CancellationToken Token { get { return _cts.Token; } }

        public bool HasError { get; private set; }

        public TaskRunner() : this(new IT.TaskScheduler()) { }

        public TaskRunner(IT.TaskScheduler scheduler)
        {
            Scheduler = scheduler;
            _task = UpdateLoop(scheduler);
        }

        private async TT.Task UpdateLoop(IT.TaskScheduler scheduler)
        {
            _isAlive = true;
            while (_isAlive)
            {
                try
                {
                    scheduler.Update();

                    var delayMilliseconds = scheduler.IsActive ? 5 : 50;
                    await TT.Task.Delay(delayMilliseconds).ConfigureAwait(false);
                    // ↑Delay なし、専用スレッドで回りっぱなしとかがいいかもしれないし。
                }
                catch (Exception ex)
                {
                    HasError = true;
                    OnError(ex);
                }
            }
        }

        public TT.Task Stop()
        {
            _cts.Cancel();
            _isAlive = false;
            return _task;
        }

        public TT.Task Task { get { return _task; } }

        public event EventHandler<Exception> Error;

        private void OnError(Exception ex)
        {
            var d = Error;
            if (d != null) d(this, ex);
        }
    }
}
