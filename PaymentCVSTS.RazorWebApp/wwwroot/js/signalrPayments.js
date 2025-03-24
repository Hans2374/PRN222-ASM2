// Global connection variable
let connection;

// Initialize SignalR connection
async function initializeSignalR() {
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
        console.log("SignalR connection already established");
        return connection;
    }

    console.log("Initializing SignalR connection");
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/paymentHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    try {
        await connection.start();
        console.log("✅ SignalR connected successfully");
        return connection;
    } catch (err) {
        console.error("❌ SignalR connection error:", err.toString());
        throw err;
    }
}

// Create payment via SignalR
async function createPaymentViaSignalR(payment) {
    if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
        throw new Error("SignalR connection not established");
    }

    console.log("Creating payment via SignalR:", payment);
    const paymentJson = JSON.stringify(payment);
    await connection.invoke("SendPayment", paymentJson);
    console.log("Payment created successfully");
}

// Update payment via SignalR
async function updatePaymentViaSignalR(payment) {
    if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
        throw new Error("SignalR connection not established");
    }

    console.log("Updating payment via SignalR:", payment);
    const paymentJson = JSON.stringify(payment);
    await connection.invoke("UpdatePayment", paymentJson);
    console.log("Payment updated successfully");
}

// Delete payment via SignalR
async function deletePaymentViaSignalR(paymentId) {
    if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
        throw new Error("SignalR connection not established");
    }

    console.log("Deleting payment via SignalR, ID:", paymentId);
    await connection.invoke("DeletePayment", paymentId);
    console.log("Payment deleted successfully");
}