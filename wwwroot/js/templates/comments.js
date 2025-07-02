export function initComments(templateId) {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/commentHub')
        .configureLogging(signalR.LogLevel.Information)
        .build();
    
    connection.on('ReceiveComment', (comment) => {
        const commentList = document.querySelector('.list-group');
        if (!commentList) return;
        const li = document.createElement('li');
        li.className = 'list-group-item';
        li.innerHTML = `
            <strong>${comment.userName}</strong> <small class="text-muted">${new Date(comment.createdAt).toLocaleString()}</small>
            <p>${comment.content}</p>
        `;
        commentList.appendChild(li);
    });

    connection.on('CommentDeleted', (data) => {
        if (data.templateId !== templateId) return;

        const commentElement = document.querySelector(`[data-comment-id="${data.commentId}"]`);
        if (commentElement) {
            commentElement.remove();
        }
    });
    
    connection.start()
        .then(() => console.log('SignalR connected for template ' + templateId))
        .catch(err => console.error('SignalR connection error:', err));
    
    return connection;
}