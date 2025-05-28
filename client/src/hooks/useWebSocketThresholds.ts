import { useWsClient } from "ws-request-hook";
import { useAtom } from "jotai";
import { useEffect, useCallback } from "react";
import { ThresholdsAtom, EvaluationsAtom, JwtAtom } from "../atoms";
import {
    ThresholdsBroadcastDto,
    AdminUpdatesThresholdsDto,
    ThresholdDto,
    ServerRespondsWithThresholds,
    ServerConfirmsThresholdUpdate,
    ServerSendsErrorMessage,
    AdminUpdatesThresholdsRequestDto,
    GetThresholdsRequestDto,
    StringConstants
} from "../generated-client";
import toast from "react-hot-toast";

export default function useWebSocketThresholds() {
    const { sendRequest, onMessage, readyState } = useWsClient();
    const [jwt] = useAtom(JwtAtom);
    const [, setThresholds] = useAtom(ThresholdsAtom);
    const [, setEvaluations] = useAtom(EvaluationsAtom);

    // Listen for threshold broadcasts
    useEffect(() => {
        if (readyState !== 1) return;

        const unsub = onMessage<ThresholdsBroadcastDto>(
            StringConstants.ThresholdsBroadcastDto,
            (dto) => {
                console.log("Threshold broadcast received:", dto);
                setThresholds(dto.updatedThresholds || []);
                setEvaluations(dto.evaluations || []);
            }
        );

        return () => unsub();
    }, [readyState, setThresholds, setEvaluations]);

    // Listen for threshold response
    useEffect(() => {
        if (readyState !== 1) return;

        const unsub = onMessage<ServerRespondsWithThresholds>(
            StringConstants.ServerRespondsWithThresholds,
            (response) => {
                console.log("Thresholds response received:", response.thresholdData);
                if (response.thresholdData) {
                    setThresholds(response.thresholdData.updatedThresholds || []);
                    setEvaluations(response.thresholdData.evaluations || []);
                }
            }
        );

        return () => unsub();
    }, [readyState, setThresholds, setEvaluations]);

    // Listen for update confirmations
    useEffect(() => {
        if (readyState !== 1) return;

        const unsub = onMessage<ServerConfirmsThresholdUpdate>(
            StringConstants.ServerConfirmsThresholdUpdate,
            (response) => {
                if (response.success) {
                    toast.success(response.message || "Thresholds updated successfully");
                }
            }
        );

        return () => unsub();
    }, [readyState]);

    // Listen for errors
    useEffect(() => {
        if (readyState !== 1) return;

        const unsub = onMessage<ServerSendsErrorMessage>(
            StringConstants.ServerSendsErrorMessage,
            (response) => {
                toast.error(response.message || "An error occurred");
                console.error("Threshold error:", response.message);
            }
        );

        return () => unsub();
    }, [readyState]);

    // Get thresholds via WebSocket
    const getThresholds = useCallback(async () => {
        if (!jwt || jwt.length < 1 || readyState !== 1) {
            return;
        }

        try {
            const request: GetThresholdsRequestDto = {
                eventType: "GetThresholdsRequestDto",
                authorization: `${jwt}`
            };

            const response = await sendRequest<GetThresholdsRequestDto, ServerRespondsWithThresholds>(
                request,
                StringConstants.ServerRespondsWithThresholds
            );

            // Handle response directly if needed
            if (response.thresholdData) {
                setThresholds(response.thresholdData.updatedThresholds || []);
                setEvaluations(response.thresholdData.evaluations || []);
            }
        } catch (error) {
            console.error("Error getting thresholds:", error);
            toast.error("Failed to get thresholds");
        }
    }, [sendRequest, jwt, readyState, setThresholds, setEvaluations]);

    // Update thresholds via WebSocket
    const updateThresholds = useCallback(async (thresholds: ThresholdDto[]) => {
        if (!jwt || jwt.length < 1 || readyState !== 1) {
            toast.error("Not connected or not authenticated");
            return;
        }

        try {
            const thresholdData: AdminUpdatesThresholdsDto = {
                thresholds: thresholds
            };

            const request: AdminUpdatesThresholdsRequestDto = {
                eventType: "AdminUpdatesThresholdsRequestDto",
                authorization: `${jwt}`,
                thresholdData: thresholdData
            };

            const response = await sendRequest<AdminUpdatesThresholdsRequestDto, ServerConfirmsThresholdUpdate>(
                request,
                StringConstants.ServerConfirmsThresholdUpdate
            );

            // Handle response directly if needed
            if (response.success) {
                toast.success(response.message || "Thresholds updated successfully");
            }
        } catch (error) {
            console.error("Error updating thresholds:", error);
            toast.error("Failed to update thresholds");
        }
    }, [sendRequest, jwt, readyState]);

    return {
        getThresholds,
        updateThresholds,
        isConnected: readyState === 1
    };
}