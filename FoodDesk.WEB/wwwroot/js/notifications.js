const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Обработка ошибок подключения
connection.onclose(error => {
    console.error("SignalR Connection Error:", error);
    // Попытка переподключения через 5 секунд
    setTimeout(() => {
        startConnection();
    }, 5000);
});

// Функция для начала соединения
async function startConnection() {
    try {
        await connection.start();
        console.log("SignalR Connected.");
    } catch (err) {
        console.error("SignalR Connection Error:", err);
        // Попытка переподключения через 5 секунд
        setTimeout(() => {
            startConnection();
        }, 5000);
    }
}

// Начало соединения
startConnection();

// Обработка уведомлений для клиентов
connection.on("ReceiveOrderNotification", (message) => {
    console.log("Received order notification:", message);
    showNotification(message);
});

// Обработка уведомлений для админов
connection.on("ReceiveAdminNotification", (message) => {
    console.log("Received admin notification:", message);
    showNotification(message);
    updateOrdersList();
});

function showNotification(message) {
    const notification = document.createElement('div');
    notification.className = 'notification';
    notification.textContent = message;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background-color: #4CAF50;
        color: white;
        padding: 15px;
        border-radius: 5px;
        z-index: 1000;
        box-shadow: 0 2px 5px rgba(0,0,0,0.2);
    `;
    document.body.appendChild(notification);

    // Удаление уведомления через 5 секунд
    setTimeout(() => {
        notification.remove();
    }, 5000);
}

async function updateOrdersList() {
    try {
        const response = await fetch('/AdminPanel/Orders/GetOrders');
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const orders = await response.json();
        console.log("Received orders:", orders);

        // Обновление списка ожидающих заказов
        const pendingOrdersList = document.getElementById('pendingOrdersList');
        if (pendingOrdersList) {
            pendingOrdersList.innerHTML = orders.pendingOrders.map(order => `
                <div class="order-item">
                    <h4>Заказ #${order.id}</h4>
                    <p>Клиент: ${order.customerName}</p>
                    <p>Сумма: ${order.totalAmount} ₽</p>
                    <form action="/AdminPanel/Orders/UpdateStatus" method="post">
                        <input type="hidden" name="__RequestVerificationToken" value="${document.querySelector('input[name="__RequestVerificationToken"]').value}">
                        <input type="hidden" name="orderId" value="${order.id}">
                        <input type="hidden" name="status" value="Confirmed">
                        <button type="submit" class="btn btn-success">Подтвердить</button>
                    </form>
                </div>
            `).join('');
        }

        // Обновление списка подтвержденных заказов
        const confirmedOrdersList = document.getElementById('confirmedOrdersList');
        if (confirmedOrdersList) {
            confirmedOrdersList.innerHTML = orders.confirmedOrders.map(order => `
                <div class="order-item">
                    <h4>Заказ #${order.id}</h4>
                    <p>Клиент: ${order.customerName}</p>
                    <p>Сумма: ${order.totalAmount} ₽</p>
                    <form action="/AdminPanel/Orders/UpdateStatus" method="post">
                        <input type="hidden" name="__RequestVerificationToken" value="${document.querySelector('input[name="__RequestVerificationToken"]').value}">
                        <input type="hidden" name="orderId" value="${order.id}">
                        <input type="hidden" name="status" value="Delivered">
                        <button type="submit" class="btn btn-primary">Доставлен</button>
                    </form>
                </div>
            `).join('');
        }

        // Обновление списка доставленных заказов
        const deliveredOrdersList = document.getElementById('deliveredOrdersList');
        if (deliveredOrdersList) {
            deliveredOrdersList.innerHTML = orders.deliveredOrders.map(order => `
                <div class="order-item">
                    <h4>Заказ #${order.id}</h4>
                    <p>Клиент: ${order.customerName}</p>
                    <p>Сумма: ${order.totalAmount} ₽</p>
                    <p>Статус: Доставлен</p>
                </div>
            `).join('');
        }
    } catch (error) {
        console.error("Error updating orders list:", error);
        showNotification("Ошибка при обновлении списка заказов");
    }
} 