import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { useAtom } from 'jotai';
import { DeviceLogsAtom, JwtAtom } from '../atoms';
import GraphFilter from './GraphFilter';
import ChartCard from './ChartCard';
import { cleanAirClient } from '../apiControllerClients';
import type { TimeRangeDto } from '../generated-client';
import { useWsClient } from 'ws-request-hook';
import {
    ServerBroadcastsLatestReqestedMeasurement,
    StringConstants
} from '../generated-client';

type TimeType = 'today' | 'weekly' | 'monthly';
type Filter = { type: TimeType; month?: number; year?: number };

const getTimeRange = (filter: Filter): { startDate: Date; endDate: Date } | null => {
    const now = new Date();
    let start: Date, end: Date;

    switch (filter.type) {
        case 'today':
            start = new Date(now);
            start.setHours(0, 0, 0, 0);
            end = new Date(now);
            end.setHours(23, 59, 59, 999);
            break;
        case 'weekly':
            end = new Date();
            start = new Date();
            start.setDate(end.getDate() - 6);
            start.setHours(0, 0, 0, 0);
            end.setHours(23, 59, 59, 999);
            break;
        case 'monthly':
            if (!filter.month || !filter.year) return null;
            start = new Date(Date.UTC(filter.year, filter.month - 1, 1));
            end = new Date(Date.UTC(filter.year, filter.month, 0, 23, 59, 59));
            break;
        default:
            return null;
    }

    return { startDate: start, endDate: end };
};

export default function Graphs() {
    const [logs, setLogs] = useAtom(DeviceLogsAtom);
    const [jwt] = useAtom(JwtAtom);
    const [filter, setFilter] = useState<Filter>({ type: 'today' });

    const { onMessage, readyState } = useWsClient();
    const allowGraphUpdate = useRef(false);

    const fetchTodayLogs = useCallback(async () => {
        if (!jwt) return;

        const now = new Date();
        const localMidnight = new Date(now);
        localMidnight.setHours(0, 0, 0, 0);
        const localEnd = new Date(now);
        localEnd.setHours(23, 59, 59, 999);

        // Convert to UTC by subtracting timezone offset
        const timezoneOffsetMs = localMidnight.getTimezoneOffset() * 60000;
        const startDate = new Date(localMidnight.getTime() - timezoneOffsetMs);
        const endDate = new Date(localEnd.getTime() - timezoneOffsetMs);

        const dto: TimeRangeDto = { startDate, endDate };
        const data = await cleanAirClient.getLogsForToday(dto, jwt);

        const parsed = data.map(log => ({
            ...log,
            timestamp: log.timestamp ? new Date(log.timestamp) : undefined,
            id: log.id || crypto.randomUUID(),
        }));
        setLogs(parsed);
    }, [jwt]);

    const fetchAverages = useCallback(async () => {
        if (!jwt) return;
        const range = getTimeRange(filter);
        if (!range) return;

        const dto: TimeRangeDto = { ...range };
        const data = await cleanAirClient.getDailyAverages(dto, jwt);

        const parsed = data.map(log => ({
            ...log,
            timestamp: log.timestamp ? new Date(log.timestamp) : undefined,
            id: log.id || crypto.randomUUID(),
        }));
        setLogs(parsed);
    }, [jwt, filter]);

    useEffect(() => {
        if (filter.type === 'today') {
            fetchTodayLogs();
        } else {
            fetchAverages();
        }
    }, [filter, fetchTodayLogs, fetchAverages]);

    useEffect(() => {
        allowGraphUpdate.current = filter.type === 'today';
    }, [filter.type]);

    useEffect(() => {
        if (!readyState) return;
        onMessage<ServerBroadcastsLatestReqestedMeasurement>(
            StringConstants.ServerBroadcastsLatestReqestedMeasurement,
            () => {
                if (allowGraphUpdate.current) {
                    fetchTodayLogs();
                }
            }
        );
    }, [readyState, fetchTodayLogs]);

    const formatChartData = useMemo(() => {
        return logs.map(log => {
            const timestamp = log.timestamp ? new Date(log.timestamp) : null;
            let time = '';

            if (timestamp) {
                time =
                    filter.type === 'today'
                        ? timestamp.toLocaleTimeString(undefined, {
                            hour: '2-digit',
                            minute: '2-digit',
                        })
                        : timestamp.toLocaleDateString(undefined, {
                            day: 'numeric',
                            month: 'short',
                        });
            }

            return {
                time,
                temperature: Number(log.temperature),
                humidity: Number(log.humidity),
                pressure: Number(log.pressure),
                airquality: Number(log.airquality),
            };
        });
    }, [logs, filter.type]);

    return (
        <div className="px-4 py-6">
            <div className="flex justify-center">
                <GraphFilter
                    onSelect={(type, month, year) => setFilter({ type, month, year })}
                    activeType={filter.type}
                />
            </div>
            <div className="graphs-container flex flex-wrap gap-6">
                <ChartCard title="Temperature (°C)" color="#f87171" dataKey="temperature" data={formatChartData} />
                <ChartCard title="Humidity (%)" color="#60a5fa" dataKey="humidity" data={formatChartData} />
                <ChartCard title="Pressure (hPa)" color="#34d399" dataKey="pressure" data={formatChartData} />
                <ChartCard title="Air Quality (PPM)" color="#fbbf24" dataKey="airquality" data={formatChartData} />
            </div>
        </div>
    );
}
