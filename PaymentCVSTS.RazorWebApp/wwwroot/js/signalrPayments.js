// wwwroot/js/signalrPayments.js

// Global connection variable
let signalRConnection;

// Initialize SignalR connection
async function initializeSignalR() {
    // Only create connection if not already connected
    if (signalRConnection && signalRConnection.state === signalR.HubConnectionState.Connected) {
        return signalRConnection;
    }

    // Create new connection
    signalRConnection = new signalR.HubConnectionBuilder()
        .withUrl("/paymentHub")
        .configureLogging(signalR.LogLevel.Information)
        .withAutomaticReconnect([0, 2000, 5000, 10000]) // Retry policy
        .build();

    // Event handlers for payments
    setupEventHandlers();

    // Start connection
    try {
        await signalRConnection.start();
        console.log("✅ SignalR connected successfully");
        return signalRConnection;
    } catch (err) {
        console.error("❌ SignalR connection error:", err);
        alert("Connection error. Please refresh the page and try again.");
        throw err;
    }
}

// Set up event handlers for SignalR events
function setupEventHandlers() {
    // Handler for receiving new payment
    signalRConnection.on("Receive_Payment", function (payment) {
        console.log("📩 Received new payment:", payment);

        // Format date for display
        const formattedDate = formatDateDisplay(payment.paymentDate);

        // Service type display
        const serviceType = payment.appointment ?
            payment.appointment.serviceType : "N/A";

        // Create new row HTML
        const newRow = `
            <tr id="${payment.paymentId}">
                <td>$${parseFloat(payment.amount).toFixed(2)}</td>
                <td>${payment.paymentStatus}</td>
                <td>${payment.paymentMethod}</td>
                <td>${formattedDate}</td>
                <td>${serviceType}</td>
                <td>
                    <a href="./Details?id=${payment.paymentId}">Details</a> |
                    <a href="./Edit?id=${payment.paymentId}">Edit</a> |
                    <a href="./Delete?id=${payment.paymentId}" class="text-danger">Delete</a>
                </td>
            </tr>
        `;

        // Insert at the beginning of the table
        $("#idPayment").prepend(newRow);

        // If we now have more rows than the page size, remove the last row
        const pageSize = 7;
        if ($("#idPayment tr").length > pageSize) {
            $("#idPayment tr:last").remove();
        }
    });

    // Handler for updated payment
    signalRConnection.on("Receive_UpdatePayment", function (payment) {
        console.log("🔄 Received updated payment:", payment);

        const formattedDate = formatDateDisplay(payment.paymentDate);
        const serviceType = payment.appointment ?
            payment.appointment.serviceType : "N/A";

        // Find existing row
        const existingRow = $(`#${payment.paymentId}`);
        if (existingRow.length) {
            // Update row content
            existingRow.html(`
                <td>$${parseFloat(payment.amount).toFixed(2)}</td>
                <td>${payment.paymentStatus}</td>
                <td>${payment.paymentMethod}</td>
                <td>${formattedDate}</td>
                <td>${serviceType}</td>
                <td>
                    <a href="./Details?id=${payment.paymentId}">Details</a> |
                    <a href="./Edit?id=${payment.paymentId}">Edit</a> |
                    <a href="./Delete?id=${payment.paymentId}" class="text-danger">Delete</a>
                </td>
            `);
        }
    });

    // Handler for deleted payment
    signalRConnection.on("Receive_DeletePayment", function (id) {
        console.log("🗑️ Payment deleted:", id);
        $(`#${id}`).fadeOut(500, function () { $(this).remove(); });
    });
}

// Format date for display (DD/MM/YYYY)
function formatDateDisplay(dateInput) {
    if (!dateInput) return "";

    // If it's already in the right format, return as is
    if (typeof dateInput === 'string' && dateInput.match(/^\d{2}\/\d{2}\/\d{4}$/)) {
        return dateInput;
    }

    const date = new Date(dateInput);
    if (isNaN(date.getTime())) return "";

    return `${String(date.getDate()).padStart(2, '0')}/${String(date.getMonth() + 1).padStart(2, '0')}/${date.getFullYear()}`;
}

// Format date for API (YYYY-MM-DD)
function formatDateForApi(dateInput) {
    if (!dateInput) return "";

    let date;
    if (typeof dateInput === 'string' && dateInput.match(/^\d{2}\/\d{2}\/\d{4}$/)) {
        // Convert from DD/MM/YYYY to Date object
        const parts = dateInput.split('/');
        date = new Date(parts[2], parts[1] - 1, parts[0]);
    } else {
        date = new Date(dateInput);
    }

    if (isNaN(date.getTime())) return "";

    return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
}

// Create payment via SignalR
async function createPaymentViaSignalR(payment) {
    try {
        const conn = await initializeSignalR();

        // Ensure date is in the correct format
        if (payment.paymentDate) {
            payment.paymentDate = formatDateForApi(payment.paymentDate);
        }

        // Serialize to JSON and send
        await conn.invoke("SendPayment", JSON.stringify(payment));
        console.log("✅ Payment created successfully");
        return true;
    } catch (err) {
        console.error("❌ Error creating payment:", err);
        alert("Error creating payment. Please try again.");
        throw err;
    }
}

// Update payment via SignalR
async function updatePaymentViaSignalR(payment) {
    try {
        console.log("Updating payment:", payment);
        const conn = await initializeSignalR();

        // Ensure date is in the correct format
        if (payment.paymentDate) {
            payment.paymentDate = formatDateForApi(payment.paymentDate);
        }

        // Make sure the payment ID is an integer
        payment.paymentId = parseInt(payment.paymentId);

        // Make sure appointment ID is an integer
        payment.appointmentId = parseInt(payment.appointmentId);

        // Deep clone to avoid reference issues
        const paymentToSend = JSON.parse(JSON.stringify(payment));

        // Remove any circular references or computed properties
        delete paymentToSend.appointment;

        console.log("Sending payment update:", paymentToSend);

        // Serialize and send
        await conn.invoke("UpdatePayment", JSON.stringify(paymentToSend));
        console.log("✅ Payment updated successfully");
        return true;
    } catch (err) {
        console.error("❌ Error updating payment:", err);
        alert("Error updating payment. Please try again.");
        throw err;
    }
}

// Delete payment via SignalR
async function deletePaymentViaSignalR(id) {
    try {
        const conn = await initializeSignalR();
        await conn.invoke("DeletePayment", parseInt(id));
        console.log("✅ Payment deleted successfully");
        return true;
    } catch (err) {
        console.error("❌ Error deleting payment:", err);
        alert("Error deleting payment. Please try again.");
        throw err;
    }
}

// Initialize when document is ready
$(document).ready(function () {
    // Initialize SignalR connection
    initializeSignalR().catch(console.error);
});

// Export functions for use in other scripts
window.initializeSignalR = initializeSignalR;
window.createPaymentViaSignalR = createPaymentViaSignalR;
window.updatePaymentViaSignalR = updatePaymentViaSignalR;
window.deletePaymentViaSignalR = deletePaymentViaSignalR;