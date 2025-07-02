export function initLikes(templateId) {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/likeHub')
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connection.on('ReceiveLikeUpdate', (data) => {
        if (data.templateId !== templateId) return;

        const likeCountElement = document.querySelector(`#like-count-${data.templateId}`);
        if (likeCountElement) {
            likeCountElement.textContent = data.likeCount;
        }
        
        const likeButton = document.querySelector(`#like-button-${data.templateId}`);
        if (likeButton) {
            fetch(`/Templates/IsLiked?templateId=${data.templateId}`, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json'
                }
            })
                .then(response => response.json())
                .then(isLiked => {
                    likeButton.textContent = isLiked ? '@Localizer["Unlike"]' : '@Localizer["Like"]';
                })
                .catch(err => console.error('Error checking like status:', err));
        }
    });

    connection.start()
        .then(() => console.log(`SignalR connected for likes on template ${templateId}`))
        .catch(err => console.error('SignalR connection error:', err));

    return connection;
}