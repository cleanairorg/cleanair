import {ThresholdStates} from "../generated-client"

export function getColorForstate(state: ThresholdStates | undefined): string {
    switch (state) {
        case ThresholdStates.Good:
            return "green";
        case ThresholdStates.WarningHigh:
        case ThresholdStates.WarningLow:
            return "yellow";
        case ThresholdStates.CriticalHigh:
        case ThresholdStates.CriticalLow:
            return "red";
        default:
            return "red"
    }
}