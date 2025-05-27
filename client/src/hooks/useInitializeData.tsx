import {useEffect} from "react";
import {cleanAirClient} from "../apiControllerClients.ts";
import {useAtom} from "jotai";
import {
    CurrentValueAtom,
    DeviceLogsAtom,
    JwtAtom
} from "../atoms.ts";
import useWebSocketThresholds from "./useWebSocketThresholds.ts";

export default function useInitializeData() {
    const [jwt] = useAtom(JwtAtom);
    const [, setDeviceLogs] = useAtom(DeviceLogsAtom)
    const [, setCurrentValue] = useAtom(CurrentValueAtom);
    const { getThresholds, isConnected } = useWebSocketThresholds();

    useEffect(() => {
        if (jwt == null || jwt.length < 1)
            return;
        cleanAirClient.getLogs(jwt).then(r => {
            setDeviceLogs(r || []);
        })
    }, [jwt])

    useEffect(() => {
        if (jwt == null || jwt.length < 1) {
            return;
        }
        cleanAirClient.getLatestMeasurement().then(r => {
            setCurrentValue(r);
        })
    }, [jwt])

    useEffect(() => {
        if (jwt == null || jwt.length < 1 || !isConnected) {
            return;
        }

        console.log("Loading thresholds via WebSocket...");
        getThresholds();
    }, [jwt, isConnected, getThresholds]);
}