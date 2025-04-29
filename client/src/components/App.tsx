import { useEffect, useState } from "react";
import { WsClientProvider } from 'ws-request-hook';
import ApplicationRoutes from "./ApplicationRoutes.tsx";
import { DevTools } from "jotai-devtools";
import 'jotai-devtools/styles.css';

// Polyfill for crypto.randomUUID
import 'crypto-browserify';

const baseUrl = import.meta.env.VITE_API_BASE_URL;
const prod = import.meta.env.PROD;

// Fallback for randomUUID if not supported natively
export const randomUid = typeof crypto.randomUUID === 'function'
    ? crypto.randomUUID()
    : 'fallback-uuid'; // use a fallback UUID

export default function App() {
    const [serverUrl, setServerUrl] = useState<string | undefined>(undefined);

    useEffect(() => {
        const finalUrl = prod
            ? `wss://${baseUrl}?id=${randomUid}`
            : `ws://${baseUrl}?id=${randomUid}`;

        setServerUrl(finalUrl);
    }, [prod, baseUrl]);

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