import {useEffect} from "react";
import {cleanAirClient} from "../apiControllerClients.ts";
import {useAtom} from "jotai";
import {CurrentValueAtom, DeviceLogsAtom, JwtAtom} from "../atoms.ts";

export default function useInitializeData() {

    const [jwt] = useAtom(JwtAtom);
    const [, setDeviceLogs] = useAtom(DeviceLogsAtom)
    const [, setCurrentValue] = useAtom(CurrentValueAtom);

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

}