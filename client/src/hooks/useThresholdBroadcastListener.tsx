import { useWsClient } from "ws-request-hook";
import { useAtom } from "jotai";
import { useEffect } from "react";
import { ThresholdsAtom, EvaluationsAtom } from "../atoms";
import { ThresholdsBroadcastDto, StringConstants, ThresholdEvaluationResult } from "../generated-client";


export default function useThresholdBroadcastListener() {
    const { onMessage, readyState } = useWsClient();
    const [, setThresholds] = useAtom(ThresholdsAtom);
    const [, setEvaluations] = useAtom(EvaluationsAtom);

    useEffect(() => {
        if (readyState !== 1) return;

        const unsub = onMessage<ThresholdsBroadcastDto>(
            StringConstants.ThresholdsBroadcastDto,
            (dto) => {
                console.log("Broadcast received:", dto);
                console.log("Updated thresholds:", dto.updatedThresholds);
                console.log("Evaluations:", dto.evaluations);
                setThresholds(dto.updatedThresholds!);
                setEvaluations(dto.evaluations!);
            }
        );

        return () => unsub();
    }, [readyState]);
}