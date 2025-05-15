using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using Svg.Editor.Events;
using Svg.Editor.Interfaces;
using Svg.Editor.Services;
using Svg.Editor.Tools;
using Svg.Interfaces;

namespace Svg.Editor.Core.Test
{
    public abstract class SvgDrawingCanvasTestBase
    {
        protected ISvgDrawingCanvas Canvas { get; set; }

        protected SchedulerProvider SchedulerProvider { get; } = new SchedulerProvider(CurrentThreadScheduler.Instance, new TestScheduler());

        [SetUp]
        public void SetUp()
        {
            SvgPlatform.Init();
            SvgEditor.Init();

            SvgEngine.Register(() => SchedulerProvider);
            SvgEngine.RegisterSingleton<IGestureRecognizer>(() => new ReactiveGestureRecognizer(SchedulerProvider));
            SvgEngine.Register<IFileLoader>(() => new FileLoader());

            Canvas = new SvgDrawingCanvas();
            Canvas.LoadTools(GetTools().ToArray());
            SetupOverride();
        }

        private IEnumerable<Func<ITool>> GetTools()
        {
            var undoRedoService = SvgEngine.Resolve<IUndoRedoService>();

            var freeDrawToolProperties = new Dictionary<string, object>
            {
                { FreeDrawingTool.DefaultStrokeWidthKey, 12 }
            };

            yield return () => new FreeDrawingTool(freeDrawToolProperties, undoRedoService);
        }

        protected virtual void SetupOverride()
        {
            
        }

        [TearDown]
        public virtual void TearDown()
        {
            Canvas.Dispose();
        }

        protected SvgDocument LoadDocument(string fileName)
        {
            var l = SvgEngine.Resolve<IFileLoader>();
            return l.Load(fileName);
        }
        
        protected async Task Rotate(params float[] relativeAnglesDegree)
        {
            await Canvas.OnEvent(new RotateEvent(0, 0, RotateStatus.Start, 3));

            var sum = 0f;
            foreach (var a in relativeAnglesDegree)
            {
                sum += a;
                await Canvas.OnEvent(new RotateEvent(a, sum, RotateStatus.Rotating, 3));
            }

            await Canvas.OnEvent(new RotateEvent(0, sum, RotateStatus.End, 0));
        }

        protected async Task Move(PointF start, PointF end, int pointerCount = 1)
        {
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, start, start, start, pointerCount));
            var delta = end - start;
            await Canvas.OnEvent(new MoveEvent(start, start, end, delta, pointerCount));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, start, start, end, pointerCount));
        }
    }
}
