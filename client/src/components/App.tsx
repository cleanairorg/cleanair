import { useEffect, useState } from "react";
import '../css/App.css';
import AppRoutes from './AppRoutes.tsx';
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

    return (
        <div className="app">
            {serverUrl && (
                <WsClientProvider url={serverUrl}>
                    <AppRoutes />
                </WsClientProvider>
            )}
            {!prod && <DevTools />}
        </div>
    );
}
