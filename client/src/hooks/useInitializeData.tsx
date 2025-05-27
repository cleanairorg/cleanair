import {useEffect} from "react";
import {cleanAirClient} from "../apiControllerClients.ts";
import { thresholdClient } from "../apiControllerClients.ts";
import {useAtom} from "jotai";
import {
    CurrentValueAtom,
    DeviceLogsAtom,
    EvaluationsAtom,
    ThresholdsAtom,
    JwtAtom
} from "../atoms.ts";

export default function useInitializeData() {

    const [jwt] = useAtom(JwtAtom);
    const [, setDeviceLogs] = useAtom(DeviceLogsAtom)
    const [, setCurrentValue] = useAtom(CurrentValueAtom);
    const [, setEvaluations] = useAtom(EvaluationsAtom);
    const [, setThresholds] = useAtom(ThresholdsAtom);

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
        if (jwt == null || jwt.length < 1) {
            return;
        }
        thresholdClient.getThresholds(jwt).then((dto) => {
            if (!dto) return;
            setThresholds(dto.updatedThresholds || []);
            setEvaluations(dto.evaluations || []);
            console.log("Initial thresholds loaded", dto);
        });
    }, [jwt]);
}