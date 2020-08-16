﻿using ETLBox.ControlFlow;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ETLBox.DataFlow
{
    public abstract class DataFlowJoinTarget<TInput> : DataFlowTask, IDataFlowDestination<TInput>
    {
        internal override Task BufferCompletion => TargetBlock.Completion;

        internal override void CompleteBufferOnPredecessorCompletion() => TargetBlock.Complete();

        internal override void FaultBufferOnPredecessorCompletion(Exception e) => TargetBlock.Fault(e);

        public virtual ITargetBlock<TInput> TargetBlock { get; }

        public void CreateLinkInInternalFlow(DataFlowTask parent)
        {
            Parent = parent;
            InternalLinkTo<TInput>(parent as IDataFlowDestination);
        }
    }
}