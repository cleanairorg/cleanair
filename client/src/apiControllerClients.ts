import {AuthClient, CleanAirClient, SubscriptionClient, ThresholdClient} from "./generated-client.ts";

const baseUrl = import.meta.env.VITE_API_BASE_URL
const prod = import.meta.env.PROD

const isRelativePath = baseUrl.startsWith('/');

export const subscriptionClient = new SubscriptionClient(
    isRelativePath ? baseUrl : (prod ? "https://" + baseUrl : "http://" + baseUrl)
);
export const cleanAirClient = new CleanAirClient(
    isRelativePath ? baseUrl : (prod ? "https://" + baseUrl : "http://" + baseUrl)
);
export const thresholdClient = new ThresholdClient(
    isRelativePath ? baseUrl : (prod ? "https://" + baseUrl : "http://" + baseUrl)
);
export const authClient = new AuthClient(
    isRelativePath ? baseUrl : (prod ? "https://" + baseUrl : "http://" + baseUrl)
);