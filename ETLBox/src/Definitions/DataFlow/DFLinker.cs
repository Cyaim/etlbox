﻿using ETLBox.ControlFlow;
using ETLBox.DataFlow.Connectors;
using System;
using System.Threading.Tasks.Dataflow;
using CF = ETLBox.ControlFlow;

namespace ETLBox.DataFlow
{
    internal class DataFlowLinker<TOutput>
    {
        public ISourceBlock<TOutput> SourceBlock { get; set; }
        public bool DisableLogging => CallingTask.DisableLogging;
        public NLog.Logger NLogger { get; set; } = CF.ControlFlow.GetLogger();
        public DataFlowTask CallingTask { get; set; }

        public DataFlowLinker(DataFlowTask callingTask, ISourceBlock<TOutput> sourceBlock)
        {
            this.CallingTask = callingTask;
            this.SourceBlock = sourceBlock;
        }

        public IDataFlowSource<TOutput> LinkTo(IDataFlowDestination<TOutput> target)
            => LinkTo<TOutput>(target);

        public IDataFlowSource<TConvert> LinkTo<TConvert>(IDataFlowDestination<TOutput> target)
        {
            SourceBlock.LinkTo(target.TargetBlock);
            //target.AddPredecessorCompletion(SourceBlock.Completion);
            if (!DisableLogging)
                NLogger.Debug(CallingTask.TaskName + $" was linked to: {((ITask)target).TaskName}", CallingTask.TaskType, "LOG", CallingTask.TaskHash, ControlFlow.ControlFlow.STAGE, ControlFlow.ControlFlow.CurrentLoadProcess?.Id);
            return target as IDataFlowSource<TConvert>;
        }

        public IDataFlowSource<TOutput> LinkTo(IDataFlowDestination<TOutput> target, Predicate<TOutput> predicate)
            => LinkTo<TOutput>(target, predicate);

        public IDataFlowSource<TConvert> LinkTo<TConvert>(IDataFlowDestination<TOutput> target, Predicate<TOutput> predicate)
        {
            SourceBlock.LinkTo(target.TargetBlock, predicate);
            //target.AddPredecessorCompletion(SourceBlock.Completion);
            if (!DisableLogging)
                NLogger.Debug(CallingTask.TaskName + $" was linked to (with predicate): {((ITask)target).TaskName}!", CallingTask.TaskType, "LOG", CallingTask.TaskHash, ControlFlow.ControlFlow.STAGE, ControlFlow.ControlFlow.CurrentLoadProcess?.Id);
            return target as IDataFlowSource<TConvert>;
        }

        public IDataFlowSource<TOutput> LinkTo(IDataFlowDestination<TOutput> target, Predicate<TOutput> rowsToKeep, Predicate<TOutput> rowsIntoVoid)
            => LinkTo<TOutput>(target, rowsToKeep, rowsIntoVoid);

        public IDataFlowSource<TConvert> LinkTo<TConvert>(IDataFlowDestination<TOutput> target, Predicate<TOutput> rowsToKeep, Predicate<TOutput> rowsIntoVoid)
        {
            SourceBlock.LinkTo(target.TargetBlock, rowsToKeep);
            //target.AddPredecessorCompletion(SourceBlock.Completion);
            if (!DisableLogging)
                NLogger.Debug(CallingTask.TaskName + $" was linked to (with predicate): {((ITask)target).TaskName}!", CallingTask.TaskType, "LOG", CallingTask.TaskHash, ControlFlow.ControlFlow.STAGE, ControlFlow.ControlFlow.CurrentLoadProcess?.Id);

            VoidDestination<TOutput> voidTarget = new VoidDestination<TOutput>();
            SourceBlock.LinkTo<TOutput>(voidTarget.TargetBlock, rowsIntoVoid);
            //voidTarget.AddPredecessorCompletion(SourceBlock.Completion);
            if (!DisableLogging)
                NLogger.Debug(CallingTask.TaskName + $" was also linked to: VoidDestination to ignore certain rows!", CallingTask.TaskType, "LOG", CallingTask.TaskHash, ControlFlow.ControlFlow.STAGE, ControlFlow.ControlFlow.CurrentLoadProcess?.Id);

            return target as IDataFlowSource<TConvert>;
        }
    }
}