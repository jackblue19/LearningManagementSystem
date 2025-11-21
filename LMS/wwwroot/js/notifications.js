// SignalR connection for real-time notifications
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .withAutomaticReconnect()
    .build();

let unreadCount = 0;

// Start connection
connection.start()
    .then(() => {
        console.log("SignalR Connected");
        loadRecentNotifications();
    })
    .catch(err => console.error("SignalR Connection Error: ", err));

// Listen for new notifications
connection.on("ReceiveNotification", (notification) => {
    console.log("New notification received:", notification);
    
    // Update badge
    unreadCount++;
    updateBadge();
    
    // Add to dropdown
    prependNotificationToDropdown(notification);
    
    // Show browser notification if permission granted
    if (Notification.permission === "granted") {
        new Notification(notification.title, {
            body: notification.message,
            icon: "/favicon.ico"
        });
    }
});

// Listen for notification read events
connection.on("NotificationRead", (notificationId) => {
    console.log("Notification marked as read:", notificationId);
    // Update UI if needed
    const notifElement = document.querySelector(`[data-notification-id="${notificationId}"]`);
    if (notifElement) {
        notifElement.classList.remove('unread');
        unreadCount = Math.max(0, unreadCount - 1);
        updateBadge();
    }
});

// Listen for notification update events
connection.on("NotificationUpdated", (notification) => {
    console.log("Notification updated:", notification);
    const notifElement = document.querySelector(`[data-notification-id="${notification.notificationId}"]`);
    if (notifElement) {
        // Update the notification in the dropdown
        const updatedHtml = createNotificationHtml({
            notificationId: notification.notificationId,
            content: `${notification.title}\n${notification.message}`,
            notiType: notification.type,
            isRead: false,
            createdAt: notification.updatedAt
        });
        notifElement.outerHTML = updatedHtml;
    }
});

// Listen for notification delete events
connection.on("NotificationDeleted", (notificationId) => {
    console.log("Notification deleted:", notificationId);
    const notifElement = document.querySelector(`[data-notification-id="${notificationId}"]`);
    if (notifElement) {
        notifElement.remove();
        
        // Check if dropdown is empty
        const notificationList = document.getElementById('notificationList');
        if (notificationList.children.length === 0) {
            notificationList.innerHTML = `
                <div class="text-center py-4 text-muted">
                    <i class="bi bi-bell-slash fs-3"></i>
                    <p class="mb-0 mt-2">Chưa có thông báo</p>
                </div>
            `;
        }
    }
});

// Update badge display
function updateBadge() {
    const badge = document.getElementById('notificationBadge');
    if (badge) {
        if (unreadCount > 0) {
            badge.textContent = unreadCount > 99 ? '99+' : unreadCount;
            badge.classList.remove('d-none');
        } else {
            badge.classList.add('d-none');
        }
    }
}

// Load recent notifications on page load
function loadRecentNotifications() {
    fetch('/api/notifications/recent')
        .then(response => response.json())
        .then(data => {
            unreadCount = data.unreadCount || 0;
            updateBadge();
            
            const notificationList = document.getElementById('notificationList');
            if (data.notifications && data.notifications.length > 0) {
                notificationList.innerHTML = data.notifications.map(n => createNotificationHtml(n)).join('');
            }
        })
        .catch(err => console.error("Error loading notifications:", err));
}

// Create notification HTML
function createNotificationHtml(notification) {
    const contentLines = (notification.content || '').split('\n', 2);
    const title = contentLines[0] || '';
    const message = contentLines[1] || '';
    
    const iconClass = getBadgeIcon(notification.notiType);
    const badgeClass = getBadgeClass(notification.notiType);
    const unreadClass = notification.isRead ? '' : 'unread bg-light';
    
    const createdAt = new Date(notification.createdAt);
    const timeAgo = formatTimeAgo(createdAt);
    
    return `
        <a href="/Common/Notifications" class="dropdown-item ${unreadClass}" data-notification-id="${notification.notificationId}" onclick="markNotificationAsRead(${notification.notificationId}, event)" style="cursor: pointer; text-decoration: none; color: inherit;">
            <div class="d-flex">
                <div class="me-2">
                    <span class="badge ${badgeClass}">
                        <i class="bi ${iconClass}"></i>
                    </span>
                </div>
                <div class="flex-grow-1">
                    <h6 class="mb-1 fw-bold">${escapeHtml(title)}</h6>
                    <p class="mb-1 small">${escapeHtml(message)}</p>
                    <small class="text-muted">${timeAgo}</small>
                </div>
            </div>
        </a>
    `;
}

// Prepend new notification to dropdown
function prependNotificationToDropdown(notification) {
    const notificationList = document.getElementById('notificationList');
    
    // Remove "no notifications" message if exists
    if (notificationList.querySelector('.text-muted')) {
        notificationList.innerHTML = '';
    }
    
    const notificationHtml = createNotificationHtml({
        notificationId: notification.notificationId || 0,
        content: `${notification.title}\n${notification.message}`,
        notiType: notification.type,
        isRead: false,
        createdAt: notification.createdAt
    });
    
    notificationList.insertAdjacentHTML('afterbegin', notificationHtml);
    
    // Keep only recent 5 notifications in dropdown
    const items = notificationList.querySelectorAll('.dropdown-item');
    if (items.length > 5) {
        for (let i = 5; i < items.length; i++) {
            items[i].remove();
        }
    }
}

// Helper functions
function getBadgeClass(type) {
    switch (type) {
        case 'success': return 'bg-success';
        case 'warning': return 'bg-warning text-dark';
        case 'danger': return 'bg-danger';
        default: return 'bg-info';
    }
}

function getBadgeIcon(type) {
    switch (type) {
        case 'success': return 'bi-check-circle-fill';
        case 'warning': return 'bi-exclamation-triangle-fill';
        case 'danger': return 'bi-x-circle-fill';
        default: return 'bi-info-circle-fill';
    }
}

function formatTimeAgo(date) {
    const seconds = Math.floor((new Date() - date) / 1000);
    
    const intervals = {
        năm: 31536000,
        tháng: 2592000,
        tuần: 604800,
        ngày: 86400,
        giờ: 3600,
        phút: 60
    };
    
    for (const [name, secondsInInterval] of Object.entries(intervals)) {
        const interval = Math.floor(seconds / secondsInInterval);
        if (interval >= 1) {
            return `${interval} ${name} trước`;
        }
    }
    
    return 'Vừa xong';
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Mark notification as read when clicked
function markNotificationAsRead(notificationId, event) {
    // Check if notification is unread
    const notifElement = document.querySelector(`[data-notification-id="${notificationId}"]`);
    if (notifElement && notifElement.classList.contains('unread')) {
        // Send request to mark as read
        fetch('/api/notifications/mark-read', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ notificationId: notificationId })
        })
        .then(response => {
            if (response.ok) {
                // Update UI immediately
                notifElement.classList.remove('unread', 'bg-light');
                unreadCount = Math.max(0, unreadCount - 1);
                updateBadge();
                
                // Notify via SignalR
                connection.invoke("MarkAsRead", notificationId)
                    .catch(err => console.error("Error invoking MarkAsRead:", err));
            }
        })
        .catch(err => console.error("Error marking notification as read:", err));
    }
}

// Request browser notification permission
if ('Notification' in window && Notification.permission === 'default') {
    Notification.requestPermission();
}
