import {AuthClient, SubscriptionClient, WeatherStationClient} from "./generated-client.ts";

const baseUrl = import.meta.env.VITE_API_BASE_URL || "";
const prod = import.meta.env.PROD;

// Brug tomme baseUrl direkte, ellers tilføj protokol
export const subscriptionClient = new SubscriptionClient(
    baseUrl === "" ? "" : (prod ? "https://" + baseUrl : "http://" + baseUrl)
);
export const weatherStationClient = new WeatherStationClient(
    baseUrl === "" ? "" : (prod ? "https://" + baseUrl : "http://" + baseUrl)
);
export const authClient = new AuthClient(
    baseUrl === "" ? "" : (prod ? "https://" + baseUrl : "http://" + baseUrl)
);