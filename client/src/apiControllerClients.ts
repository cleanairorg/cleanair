import {AuthClient, SubscriptionClient, WeatherStationClient} from "./generated-client.ts";

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL
const prod = import.meta.env.PROD

const isRelativePath = apiBaseUrl.startsWith('/');

const baseUrl = isRelativePath ? apiBaseUrl : (prod ? "http://" + apiBaseUrl : "http://" + apiBaseUrl);

export const subscriptionClient = new SubscriptionClient(baseUrl);
export const weatherStationClient = new WeatherStationClient(baseUrl);
export const authClient = new AuthClient(baseUrl);