import '../css/MeasurementAlerts.css';
import { useAtom } from "jotai";
import { EvaluationsAtom } from "../atoms";
import { ThresholdStates } from "../generated-client";

import danger from "../assets/danger.png";
import thumbUp from "../assets/thumb-up.png";

const getClass = (state: ThresholdStates) => {
    if (state === ThresholdStates.CriticalHigh || state === ThresholdStates.CriticalLow) return "critical";
    if (state === ThresholdStates.WarningHigh || state === ThresholdStates.WarningLow) return "warning";
    return "good";
};

const getMessage = (metric: string, state: ThresholdStates): string => {
    const label = metric.toUpperCase();
    if (state === ThresholdStates.CriticalHigh || state === ThresholdStates.WarningHigh) return `${label} is too high`;
    if (state === ThresholdStates.CriticalLow || state === ThresholdStates.WarningLow) return `${label} is too low`;
    return `${label} is fine`;
};

export default function MeasurementAlerts() {
    const [evaluations] = useAtom(EvaluationsAtom);

    const alerts = evaluations
        .filter(e => e.state !== ThresholdStates.Good)
        .sort((a, b) => b.state - a.state);

    return (
        <aside className="measurement-alerts">
            <h2 className="section-title">MEASUREMENT ALERTS</h2>

            {alerts.length === 0 ? (
                <div className="alert-card good">
                    <img src={thumbUp} className="alert-icon" />
                </div>
            ) : (
                <div className="alert-grid">
                    {alerts.map((alert, i) => (
                        <div key={i} className={`alert-card ${getClass(alert.state)}`}>
                            <img src={danger} className="alert-icon" alt="Danger" />
                            <div>{getMessage(alert.metric, alert.state)}</div>
                        </div>
                    ))}
                </div>
            )}
        </aside>
    );
}
