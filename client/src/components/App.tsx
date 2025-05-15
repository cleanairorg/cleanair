import { useEffect, useState } from "react";
import '../css/App.css';
import Header from './Header';
import DeviceSettings from './DeviceSettings';
import CurrentValues from './CurrentValues';
import MeasurementAlerts from './MeasurementAlerts';
import Graphs from './Graphs';
import { v4 as uuidv4 } from 'uuid';
import { DevTools } from "jotai-devtools";
import 'jotai-devtools/styles.css';
import { WsClientProvider } from 'ws-request-hook';

const baseUrl = import.meta.env.VITE_API_WS_URL;
const prod = import.meta.env.PROD;
export const randomUid = uuidv4();

export default function App() {
    const [serverUrl, setServerUrl] = useState<string | undefined>(undefined);

    useEffect(() => {
        const finalUrl = `ws://${baseUrl}?id=${randomUid}`;
        console.log("WebSocket URL:", finalUrl);
        setServerUrl(finalUrl);
    }, [prod, baseUrl]);

    const AppContent = (
        <div className="app">
            <Header />
            <main className="main-layout">
                <DeviceSettings />
                <CurrentValues />
                <MeasurementAlerts />
            </main>
            <Graphs />
        </div>
    );

    return (
        <>
            {serverUrl ? (
                <WsClientProvider url={serverUrl}>
                    {AppContent}
                </WsClientProvider>
            ) : (
                AppContent
            )}
            {!prod && <DevTools />}
        </>
    );
}
