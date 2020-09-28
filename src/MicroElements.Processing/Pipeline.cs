// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks.Dataflow;

namespace MicroElements.Processing
{
    // ALSO LOOK https://michaelscodingspot.com/pipeline-pattern-tpl-dataflow/
    internal static class Pipeline2
    {
        public static (BufferBlock<A> First, TransformBlock<A, B> Last) Then<A, B>(
            this BufferBlock<A> block, TransformBlock<A, B> next)
        {
            block.LinkTo(next, new DataflowLinkOptions { PropagateCompletion = true });
            return (block, next);
        }

        public static (TransformBlock<A, B> First, TransformBlock<B, C> Last) Then<A, B, C>(
            this TransformBlock<A, B> block, TransformBlock<B, C> next)
        {
            block.LinkTo(next, new DataflowLinkOptions { PropagateCompletion = true });
            return (block, next);
        }

        public static (BufferBlock<A> First, TransformBlock<C, D> Last) Then<A, B, C, D>(
            this (BufferBlock<A>, TransformBlock<B, C>) blocks, TransformBlock<C, D> next)
        {
            blocks.Item2.LinkTo(next, new DataflowLinkOptions { PropagateCompletion = true });
            return (blocks.Item1, next);
        }

        public static (TransformBlock<A, B> First, TransformBlock<C, D> Last) Then<A, B, C, D>(
            this (TransformBlock<A, B>, TransformBlock<B, C>) blocks, TransformBlock<C, D> next)
        {
            blocks.Item2.LinkTo(next, new DataflowLinkOptions { PropagateCompletion = true });
            return (blocks.Item1, next);
        }

        public static (BufferBlock<A> First, ActionBlock<C> Last) Then<A, B, C>(
            this (BufferBlock<A>, TransformBlock<B, C>) blocks, ActionBlock<C> next)
        {
            blocks.Item2.LinkTo(next, new DataflowLinkOptions { PropagateCompletion = true });
            return (blocks.Item1, next);
        }

        public static (TransformBlock<A, B> First, ActionBlock<C> Last) Then<A, B, C>(
            this (TransformBlock<A, B>, TransformBlock<B, C>) blocks, ActionBlock<C> next)
        {
            blocks.Item2.LinkTo(next, new DataflowLinkOptions { PropagateCompletion = true });
            return (blocks.Item1, next);
        }
    }

    public class StepContext<A, B>
    {
        public StepContext(A input, CancellationToken token, TaskCompletionSource<B> tcs)
        {
            Input = input;
            Token = token;
            TaskCompletionSource = tcs;
        }

        public A Input { get; set; }

        public CancellationToken Token { get; set; }

        public TaskCompletionSource<B> TaskCompletionSource { get; set; }
    }

    public static class PipelineBuilder
    {
        public static PipelineBuilder<A> Input<A>()
        {
            return new PipelineBuilder<A>();
        }

        public static PipelineBuilder<A, B> AddStep<A, B>(Func<A, B> func, int threadLimit = 1)
        {
            return new PipelineBuilder<A, B>(func, threadLimit);
        }
    }

    public class PipelineBuilder<A>
    {
        public PipelineBuilder<A, B> AddStep<B>(Func<A, B> func, int threadLimit = 1)
        {
            return new PipelineBuilder<A, B>(func, threadLimit);
        }
    }

    public class PipelineBuilder<A, B>
    {
        public BufferBlock<A> Input { get; }

        public TransformBlock<A, B> First { get; }

        public PipelineBuilder(Func<A, B> func, int threadLimit)
        {
            Input = new BufferBlock<A>();

            var parallelOptions = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = threadLimit };
            First = new TransformBlock<StepContext<A, B>, StepContext<A, B>>(context =>
            {
                B result = func(context.Input);
                context.TaskCompletionSource.SetResult(result);
                return new StepContext<A, B>(context.Input, context.Token, );
            }, parallelOptions);
            Input.LinkTo(First);
        }

        public PipelineBuilder<A, A, B, C> AddStep<C>(Func<B, C> func, int threadLimit = 1)
        {
            return new PipelineBuilder<A, A, B, C>(Input, First, func, threadLimit);
        }

        public Func<A, CancellationToken, Task<B>> CreateWaitablePipeline()
        {
            return (a, token) =>
            {
                Input.Post(a);
                return First.Completion;
            }
        }
    }

    public class PipelineBuilder<A, B, C, D>
    {
        public BufferBlock<A> Input { get; }

        public TransformBlock<B, C> Prev { get; }

        public TransformBlock<C, D> Next { get; }

        public PipelineBuilder(BufferBlock<A> first, TransformBlock<B, C> prev, Func<C, D> func, int threadLimit)
        {
            Input = first;
            Prev = prev;

            var parallelOptions = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = threadLimit };
            Next = new TransformBlock<C, D>(func, parallelOptions);
            Prev.LinkTo(Next);
        }
    }

    public static class PipelineSample
    {
        public static async Task ExecutePipeline()
        {
            var pipeline =
                    PipelineBuilder
                        .Input<int>()
                        .AddStep(MultBy2, 1)
                        .AddStep(IntToString, 1)
                        .C
     ;



            pipeline(5);
            pipelineBuilder.First.Input.Completion
        }

        public static async Task ExecutePipeline2()
        {
            var step1 = new TransformBlock<StepContext<string, bool>, StepContext<string, bool>>((tc) =>
                new StepContext<string, bool>(FindMostCommon(tc.Input), CancellationToken.None, tc.TaskCompletionSource));

            var step2 = new TransformBlock<StepContext<string, bool>, StepContext<int, bool>>((tc) =>
                new StepContext<int, bool>(tc.Input.Length, CancellationToken.None, tc.TaskCompletionSource));

            var step3 = new TransformBlock<StepContext<int, bool>, StepContext<bool, bool>>((tc) =>
                new StepContext<bool, bool>(tc.Input % 2 == 1, CancellationToken.None, tc.TaskCompletionSource));

            var setResultStep = new ActionBlock<StepContext<bool, bool>>((tc) => tc.TaskCompletionSource.SetResult(tc.Input));

            step1.LinkTo(step2, new DataflowLinkOptions());
            step2.LinkTo(step3, new DataflowLinkOptions());
            step3.LinkTo(setResultStep, new DataflowLinkOptions());
        }

        public static int MultBy2(int num)
        {
            return num * 2;
        }

        public static string IntToString(int num)
        {
            return num.ToString();
        }

        private static string FindMostCommon(string input)
        {
            return input.Split(' ')
                .GroupBy(word => word)
                .OrderBy(group => group.Count())
                .Last()
                .Key;
        }
    }
}
