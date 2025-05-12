import {AuthClient, SubscriptionClient, WeatherStationClient} from "./generated-client.ts";

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL
const prod = import.meta.env.PROD

// For REST API
const baseUrl = prod
    ? apiBaseUrl  // I produktion: Bruges "/api" (relativ sti)
    : `http://${apiBaseUrl}`; // I udvikling: Bruges værdi fra .env.development

console.log("API Base URL:", baseUrl);

export const subscriptionClient = new SubscriptionClient(baseUrl);
export const weatherStationClient = new WeatherStationClient(baseUrl);
export const authClient = new AuthClient(baseUrl);