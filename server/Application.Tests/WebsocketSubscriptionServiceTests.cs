
using Application.Interfaces.Infrastructure.Websocket;
using Application.Services;
using Moq;


namespace Application.Tests
{
    [TestFixture]
    public class WebsocketSubscriptionServiceTests
    {
        private Mock<IConnectionManager> _connectionManagerMock = null!;
        private WebsocketSubscriptionService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _connectionManagerMock = new Mock<IConnectionManager>();
            _service = new WebsocketSubscriptionService(_connectionManagerMock.Object);
        }

        [Test]
        public async Task SubscribeToTopic_ShouldCallAddToTopic_ForEachTopic()
        {
            // Arrange
            var clientId = "client-1";
            var topics = new List<string> { "topicA", "topicB" };

            // Act
            await _service.SubscribeToTopic(clientId, topics);

            // Assert
            foreach (var topic in topics)
            {
                _connectionManagerMock.Verify(m => m.AddToTopic(topic, clientId), Times.Once);
            }
        }

        [Test]
        public async Task UnsubscribeFromTopic_ShouldCallRemoveFromTopic_ForEachTopic()
        {
            // Arrange
            var clientId = "client-2";
            var topics = new List<string> { "topicX", "topicY", "topicZ" };

            // Act
            await _service.UnsubscribeFromTopic(clientId, topics);

            // Assert
            foreach (var topic in topics)
            {
                _connectionManagerMock.Verify(m => m.RemoveFromTopic(topic, clientId), Times.Once);
            }
        }

        [Test]
        public async Task SubscribeToTopic_ShouldDoNothing_IfListIsEmpty()
        {
            // Arrange
            var clientId = "client-3";
            var emptyTopics = new List<string>();

            // Act
            await _service.SubscribeToTopic(clientId, emptyTopics);

            // Assert
            _connectionManagerMock.Verify(m => m.AddToTopic(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task UnsubscribeFromTopic_ShouldDoNothing_IfListIsEmpty()
        {
            // Arrange
            var clientId = "client-4";
            var emptyTopics = new List<string>();

            // Act
            await _service.UnsubscribeFromTopic(clientId, emptyTopics);

            // Assert
            _connectionManagerMock.Verify(m => m.RemoveFromTopic(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
