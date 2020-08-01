﻿using ETLBox.ControlFlow;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ETLBox.DataFlow
{
    public abstract class DataFlowTransformation<TInput, TOutput> : DataFlowTask, ITask, IDataFlowTransformation<TInput, TOutput>
    {
        public virtual ITargetBlock<TInput> TargetBlock { get; }
        public virtual ISourceBlock<TOutput> SourceBlock { get; }

        //protected List<Task> PredecessorCompletions { get; set; } = new List<Task>();

        //public void AddPredecessorCompletion(Task completion)
        //{
        //    PredecessorCompletions.Add(completion);
        //    completion.ContinueWith(t => CheckCompleteAction());
        //}

        //protected void CheckCompleteAction()
        //{
        //    Task.WhenAll(PredecessorCompletions).ContinueWith(t =>
        //    {
        //        if (!TargetBlock.Completion.IsCompleted)
        //        {
        //            if (t.IsFaulted) TargetBlock.Fault(t.Exception.InnerException);
        //            else TargetBlock.Complete();
        //        }
        //    });
        //}#

        protected override Task BufferCompletion => TargetBlock.Completion;

        protected override void CompleteOrFaultBuffer(Task t)
        {
            if (!TargetBlock.Completion.IsCompleted)
                {
                if (t.IsFaulted)
                {
                    TargetBlock.Fault(t.Exception.Flatten());
                    throw t.Exception.Flatten();
                }
                else TargetBlock.Complete();
                }
        }

        protected override void FaultBuffer(Exception e)
        {
            TargetBlock.Fault(e);
        }

        protected override void LinkBuffers(DataFlowTask successor)
        {
            var s = successor as IDataFlowLinkTarget<TOutput>;
            this.SourceBlock.LinkTo<TOutput>(s.TargetBlock);
            //s.AddPredecessorCompletion(SourceBlock.Completion);
        }

        public IDataFlowLinkSource<TOutput> LinkTo(IDataFlowLinkTarget<TOutput> target)
        => (new DataFlowLinker<TOutput>(this, SourceBlock)).LinkTo(target);

        public IDataFlowLinkSource<TOutput> LinkTo(IDataFlowLinkTarget<TOutput> target, Predicate<TOutput> predicate)
            => (new DataFlowLinker<TOutput>(this, SourceBlock)).LinkTo(target, predicate);

        public IDataFlowLinkSource<TOutput> LinkTo(IDataFlowLinkTarget<TOutput> target, Predicate<TOutput> rowsToKeep, Predicate<TOutput> rowsIntoVoid)
            => (new DataFlowLinker<TOutput>(this, SourceBlock)).LinkTo(target, rowsToKeep, rowsIntoVoid);

        public IDataFlowLinkSource<TConvert> LinkTo<TConvert>(IDataFlowLinkTarget<TOutput> target)
            => (new DataFlowLinker<TOutput>(this, SourceBlock)).LinkTo<TConvert>(target);

        public IDataFlowLinkSource<TConvert> LinkTo<TConvert>(IDataFlowLinkTarget<TOutput> target, Predicate<TOutput> predicate)
            => (new DataFlowLinker<TOutput>(this, SourceBlock)).LinkTo<TConvert>(target, predicate);

        public IDataFlowLinkSource<TConvert> LinkTo<TConvert>(IDataFlowLinkTarget<TOutput> target, Predicate<TOutput> rowsToKeep, Predicate<TOutput> rowsIntoVoid)
            => (new DataFlowLinker<TOutput>(this, SourceBlock)).LinkTo<TConvert>(target, rowsToKeep, rowsIntoVoid);
    }
}
