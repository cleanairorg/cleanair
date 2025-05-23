import { useEffect } from 'react';
import { useWsClient } from 'ws-request-hook';
import { ThresholdsBroadcastDto, StringConstants } from '../generated-client';
import { useSetAtom } from 'jotai';
import { ThresholdsAtom, ThresholdEvaluationsAtom } from '../atoms';


export default function useThresholdBroadcastListener() {
    const  {onMessage, readyState} = useWsClient();
    const setThresholds = useSetAtom(ThresholdsAtom);
    const setEvaluations = useSetAtom(ThresholdEvaluationsAtom);
    
    useEffect(() => {
        if (readyState !== 1) return;
        onMessage<ThresholdsBroadcastDto>(StringConstants.ThresholdsBroadcastDto, (dto)=> {
            setThresholds(dto.updatedThresholds);
            setEvaluations(dto.evaluations);
        });
    }, [readyState]);
}