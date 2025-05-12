import { useEffect, useState } from "react";
import { WsClientProvider } from 'ws-request-hook';
import ApplicationRoutes from "./ApplicationRoutes.tsx";
import { DevTools } from "jotai-devtools";
import 'jotai-devtools/styles.css';
import { v4 as uuidv4 } from 'uuid';

const wsUrl = import.meta.env.VITE_API_WS_URL
const prod = import.meta.env.PROD;

export const randomUid = uuidv4();

export default function App() {
    const [serverUrl, setServerUrl] = useState<string | undefined>(undefined);

    useEffect(() => {
        // WebSocket direkte på serverens IP og port
        const finalUrl = `ws://${wsUrl}?id=${randomUid}`;
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
