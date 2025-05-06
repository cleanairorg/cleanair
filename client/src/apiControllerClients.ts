import {AuthClient, SubscriptionClient, WeatherStationClient} from "./generated-client.ts";

const baseUrl = import.meta.env.VITE_API_BASE_URL
const prod = import.meta.env.PROD

const isRelativePath = baseUrl.startsWith('/');

if (isRelativePath) {
    throw new Error("VITE_API_BASE_URL must be an absolute URL.");
}

export const subscriptionClient = new SubscriptionClient(
    prod ? "https://" + baseUrl : "http://" + baseUrl
);
export const weatherStationClient = new WeatherStationClient(
    prod ? "https://" + baseUrl : "http://" + baseUrl
);
export const authClient = new AuthClient(
    prod ? "https://" + baseUrl : "http://" + baseUrl
);