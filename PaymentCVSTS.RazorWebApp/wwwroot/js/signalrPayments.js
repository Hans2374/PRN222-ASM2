// This file should be saved as wwwroot/js/signalrPayments.js

// Global connection object
let paymentConnection;

// Initialize the SignalR connection
function initializeSignalR() {
    paymentConnection = new signalR.HubConnectionBuilder()
        .withUrl("/paymentHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    console.log("SignalR connection created");

    // Start the connection
    return paymentConnection.start()
        .then(() => {
            console.log("✅ SignalR connected successfully");
            return paymentConnection;
        })
        .catch(err => {
            console.error("❌ SignalR connection error:", err.toString());
            throw err;
        });
}

// Function to create a payment via SignalR
function createPaymentViaSignalR(payment) {
    console.log("Creating payment via SignalR:", payment);

    // Make sure we have a connection
    if (!paymentConnection || paymentConnection.state !== signalR.HubConnectionState.Connected) {
        console.error("SignalR not connected");
        return Promise.reject("SignalR not connected");
    }

    // Convert payment to JSON string and send via SignalR
    return paymentConnection.invoke("SendPayment", JSON.stringify(payment))
        .then(() => {
            console.log("✅ Payment creation request sent via SignalR");
            return true;
        })
        .catch(err => {
            console.error("❌ Error sending payment via SignalR:", err);
            throw err;
        });
}

// Function to update a payment via SignalR
function updatePaymentViaSignalR(payment) {
    console.log("Updating payment via SignalR:", payment);

    // Make sure we have a connection
    if (!paymentConnection || paymentConnection.state !== signalR.HubConnectionState.Connected) {
        console.error("SignalR not connected");
        return Promise.reject("SignalR not connected");
    }

    // Convert payment to JSON string and send via SignalR
    return paymentConnection.invoke("UpdatePayment", JSON.stringify(payment))
        .then(() => {
            console.log("✅ Payment update request sent via SignalR");
            return true;
        })
        .catch(err => {
            console.error("❌ Error updating payment via SignalR:", err);
            throw err;
        });
}

// Function to delete a payment via SignalR
function deletePaymentViaSignalR(paymentId) {
    console.log("Deleting payment via SignalR:", paymentId);

    // Make sure we have a connection
    if (!paymentConnection || paymentConnection.state !== signalR.HubConnectionState.Connected) {
        console.error("SignalR not connected");
        return Promise.reject("SignalR not connected");
    }

    // Send delete request via SignalR
    return paymentConnection.invoke("DeletePayment", paymentId)
        .then(() => {
            console.log("✅ Payment deletion request sent via SignalR");
            return true;
        })
        .catch(err => {
            console.error("❌ Error deleting payment via SignalR:", err);
            throw err;
        });
}