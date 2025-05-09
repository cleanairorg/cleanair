import { useEffect, useState } from "react";
import { WsClientProvider } from 'ws-request-hook';
import ApplicationRoutes from "./ApplicationRoutes.tsx";
import { DevTools } from "jotai-devtools";
import 'jotai-devtools/styles.css';
import { v4 as uuidv4 } from 'uuid'; 

const prod = import.meta.env.PROD;

export const randomUid = uuidv4();

export default function App() {
    const [serverUrl, setServerUrl] = useState<string | undefined>(undefined);

    useEffect(() => {
        const finalUrl = `ws://79.76.52.174:8181?id=${randomUid}`;
        console.log("Connecting to WebSocket:", finalUrl);
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
