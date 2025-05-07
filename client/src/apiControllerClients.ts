import {AuthClient, SubscriptionClient, WeatherStationClient} from "./generated-client.ts";

const baseUrl = import.meta.env.VITE_API_BASE_URL || "";
const prod = import.meta.env.PROD;

// Håndterer relative stier
const isRelativePath = baseUrl.startsWith('/');

export const subscriptionClient = new SubscriptionClient(
    isRelativePath ? baseUrl : (prod ? "https://" + baseUrl : "http://" + baseUrl)
);
export const weatherStationClient = new WeatherStationClient(
    isRelativePath ? baseUrl : (prod ? "https://" + baseUrl : "http://" + baseUrl)
);
export const authClient = new AuthClient(
    isRelativePath ? baseUrl : (prod ? "https://" + baseUrl : "http://" + baseUrl)
);