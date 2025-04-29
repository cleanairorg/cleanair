import { WsClientProvider } from 'ws-request-hook';
import { useEffect, useState } from "react";
import ApplicationRoutes from "./ApplicationRoutes.tsx";
import { DevTools } from "jotai-devtools";
import 'jotai-devtools/styles.css';

const baseUrl = import.meta.env.VITE_API_BASE_URL;
const prod = import.meta.env.PROD;

export default function App() {
    const [serverUrl, setServerUrl] = useState<string | undefined>(undefined);

    useEffect(() => {
        // Call randomUUID inside useEffect to ensure it runs in the browser
        const id = crypto.randomUUID ? crypto.randomUUID() : 'fallback-uuid';
        const finalUrl = prod
            ? `wss://${baseUrl}?id=${id}`
            : `ws://${baseUrl}?id=${id}`;
        setServerUrl(finalUrl);
    }, []);

    return (
        <>
            {serverUrl && (
                <WsClientProvider url={serverUrl}>
                    <ApplicationRoutes />
                </WsClientProvider>
            )}
            {!prod && <DevTools />}
        </>
    );
}