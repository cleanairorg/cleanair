import '../css/CurrentValues.css';
import {cleanAirClient} from "../apiControllerClients.ts";
import { useWsClient } from "ws-request-hook";
import {useEffect} from "react";
import {
    ServerBroadcastsLatestReqestedMeasurement,
    StringConstants,
} from "../generated-client.ts";
import { useAtom } from "jotai";
import {CurrentValueAtom, EvaluationsAtom, JwtAtom, UserInfoAtom} from "../atoms.ts";
import toast from "react-hot-toast";
import {getColorForstate} from "../utils/thresholdUtils.ts";

export default function CurrentValues() {

    const [jwt] = useAtom(JwtAtom);
    const [userInfo,] = useAtom(UserInfoAtom);
    const { onMessage, readyState } = useWsClient();
    const [currentValue, setCurrentValue] = useAtom(CurrentValueAtom);
    const [evaluations] = useAtom(EvaluationsAtom);


    useEffect(() => {
        if (readyState != 1 || jwt == null || jwt.length < 1){
            return;
        }
        try {
            onMessage<ServerBroadcastsLatestReqestedMeasurement>(
                StringConstants.ServerBroadcastsLatestReqestedMeasurement, (dto) => {
                    toast.success("New measurements are now available");
                    setCurrentValue(dto.latestMeasurement);
                }
            )
        } catch (e){
            toast.error("Error while getting measurements");
        }
    }, [readyState]);

    return (
        <section className="current-values">
            <h2 className="section-title">CURRENT VALUES</h2>
            {currentValue ? (
                ["temperature", "humidity", "airquality", "pressure"].map((metric) => {
                    const evalResult = evaluations.find((e) => e.metric === metric);
                    const colorClass = getColorForstate(evalResult?.state);

                    let label = "";
                    let value = "";

                    switch (metric) {
                        case "temperature":
                            label = "Temperature:";
                            value = `${currentValue.temperature} °`;
                            break;
                        case "humidity":
                            label = "Humidity:";
                            value = `${currentValue.humidity} %`;
                            break;
                        case "airquality":
                            label = "Air Quality:";
                            value = `${currentValue.airquality} ppm`;
                            break;
                        case "pressure":
                            label = "Air pressure:";
                            value = `${currentValue.pressure} hPa`;
                            break;
                    }

                    return (
                        <div key={metric} className={`value ${colorClass}`}>
                            <span className={`circle ${colorClass}`}></span>
                            {label} {value}
                        </div>
                    );
                })
            ) : (
                <div>Loading measurements...</div>
            )}
            {userInfo?.role === "admin" && (
                <>
            <button className="app evaluate-button" onClick={() => {
                cleanAirClient.getMeasurementNow(jwt).then(success => {
                    toast.success("Request sent for new measurements");
                }).catch(error => {
                    toast.error("Error getting new measurements");
                    console.error(error);
                });
            }}>
                New Evaluation Now
            </button>
                </>
            )}
        </section>
    );
}

