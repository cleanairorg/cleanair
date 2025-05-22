import '../css/CurrentValues.css';
import { useWsClient } from "ws-request-hook";
import {useEffect} from "react";
import {
    ServerBroadcastsLatestReqestedMeasurement,
    StringConstants,
} from "../generated-client.ts";
import { useAtom } from "jotai";
import {CurrentValueAtom} from "../atoms.ts";
import toast from "react-hot-toast";

export default function CurrentValues() {

    const { onMessage, readyState } = useWsClient();
    const [currentValue, setCurrentValue] = useAtom(CurrentValueAtom);


    useEffect(() => {
        if (readyState){
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
            { currentValue ? (
                <>
                    <div className="value yellow">Temperature: {currentValue!.temperature } {currentValue!.unit}°</div>
                    <div className="value green">Humidity: {currentValue!.humidity} </div>
                    <div className="value red">Air Quality: {currentValue!.airquality} </div>
                    <div className="value green">Air pressure: {currentValue!.pressure} </div>
                </>
            ) : (
                <div>Loading measurements...</div>
            )}

            <div className="last-week">Last Week</div>
        </section>
    );
}
