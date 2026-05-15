using NUnit.Framework;
using SentryGame.UI;
using System.Reflection;

namespace SentryGame.Tests.EditMode
{
    public sealed class SnakeSceneControllerTests
    {
        [Test]
        public void RandomStallDurationIsOneSecond()
        {
            var field = typeof(SnakeSceneController).GetField("RandomStallMilliseconds", BindingFlags.Static | BindingFlags.NonPublic);

            Assert.IsNotNull(field, "贪吃蛇随机卡顿时长常量不存在");
            Assert.That(field.GetRawConstantValue(), Is.EqualTo(1000));
        }

        [Test]
        public void RandomCrashOnlyChecksEverySecondMove()
        {
            var field = typeof(SnakeSceneController).GetField("RandomCrashStepInterval", BindingFlags.Static | BindingFlags.NonPublic);

            Assert.IsNotNull(field, "贪吃蛇随机崩溃步数间隔常量不存在");
            Assert.That(field.GetRawConstantValue(), Is.EqualTo(2));
        }
    }
}
