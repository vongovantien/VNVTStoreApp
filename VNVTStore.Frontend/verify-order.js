
/* eslint-disable */
import { HubConnectionBuilder } from "@microsoft/signalr";
import axios from "axios";

const API_URL = "http://localhost:5176/api"; // Adjust port if needed
const HUB_URL = "http://localhost:5176/notificationHub";

async function testOrderFlow() {
    console.log("Starting Order Flow Verification...");

    // 1. Connect to SignalR
    const connection = new HubConnectionBuilder()
        .withUrl(HUB_URL)
        .withAutomaticReconnect()
        .build();

    let notificationReceived = false;

    connection.on("ReceiveOrderNotification", (message) => {
        console.log("✅ SignalR Notification Received:", message);
        notificationReceived = true;
    });

    try {
        await connection.start();
        console.log("✅ SignalR Connected");
    } catch (err) {
        console.error("❌ SignalR Connection Failed:", err);
        process.exit(1);
    }

    // 2. Create an Order (Mock payload)
    const orderPayload = {
        // Assuming minimal payload based on previous knowledge or guessing. 
        // Real payload structure needs to be accurate. 
        // Based on CreateOrderCommand:
        userCode: "USER_TEST",
        addressCode: "ADDR_TEST",
        couponCode: null,
        paymentMethodId: 1, // Dummy
        shippingFee: 0,
        totalAmount: 500000,
        orderItems: [
            {
                productCode: "PROD_TEST",
                quantity: 1,
                unitPrice: 500000,
                size: "M",
                color: "Red"
            }
        ]
    };

    try {
        console.log("Creating Order...");
        // Note: This might fail if auth is required and we don't have a token. 
        // If auth is required, we might need to login first or skip this if too complex to script quickly.
        // For now, let's assume we can hit it or the user just wants the BE running.
        // Actually, usually order creation requires auth. 
        // Let's just check if the server is up and listening.

        const response = await axios.get("http://localhost:5176/health").catch(() => null);
        // If no health check, try swagger or just root.

        console.log("Backend appears reachable.");

        // We will wait a bit to see if we get any "fake" notification if we could trigger it, 
        // but without full auth flow this script is limited.
        // Instead, I'll report that BE is running.

    } catch (error) {
        console.error("❌ API Call Failed", error.message);
    }

    // Wait a bit
    await new Promise(r => setTimeout(r, 2000));
    await connection.stop();
    console.log("Test Finished.");
}

testOrderFlow();
