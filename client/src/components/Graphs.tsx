import { useEffect, useMemo, useState, useCallback } from 'react';
import { useAtom } from 'jotai';
import { DeviceLogsAtom, JwtAtom, DeviceIdAtom } from '../atoms';
import GraphFilter from './GraphFilter';
import type { Devicelog, TimeRangeDto } from '../generated-client';
import ChartCard from './ChartCard';
import { cleanAirClient } from '../apiControllerClients';

export default function Graphs() {
    const [logs, setLogs] = useAtom(DeviceLogsAtom);
    const [deviceId] = useAtom(DeviceIdAtom);
    const [jwt] = useAtom(JwtAtom);
    const [filter, setFilter] = useState<{ type: 'today' | 'weekly' | 'monthly'; month?: number; year?: number }>({
        type: 'today',
    });

    const fetchGraphData = useCallback(async () => {
        if (!jwt) return;

        let startDate: Date;
        let endDate: Date;
        const now = new Date();

        if (filter.type === 'today') {
            startDate = new Date(now);
            startDate.setHours(0, 0, 0, 0);
            endDate = new Date(now);
            endDate.setHours(23, 59, 59, 999);
        } else if (filter.type === 'weekly') {
            endDate = new Date();
            startDate = new Date();
            startDate.setDate(endDate.getDate() - 6);
            startDate.setHours(0, 0, 0, 0);
            endDate.setHours(23, 59, 59, 999);
        } else if (filter.type === 'monthly' && filter.month && filter.year) {
            startDate = new Date(Date.UTC(filter.year, filter.month - 1, 1));
            endDate = new Date(Date.UTC(filter.year, filter.month, 0, 23, 59, 59));
        } else {
            return;
        }

        const dto: TimeRangeDto = { startDate, endDate, deviceId };

        let result: any[] = [];

        if (filter.type === 'today') {
            result = await cleanAirClient.getLogsForToday(dto);
        } else {
            result = await cleanAirClient.getDailyAverages(dto);
        }

        const transformed: Devicelog[] = result.map((r) => ({
            timestamp: r.timestamp
                ? new Date(r.timestamp)
                : r.date
                    ? new Date(r.date)
                    : undefined,
            temperature: r.temperature ?? r.avgTemperature,
            humidity: r.humidity ?? r.avgHumidity,
            pressure: r.pressure ?? r.avgPressure,
            airquality: r.airquality ?? r.avgAirQuality,
            id: r.id || crypto.randomUUID(),
            unit: r.unit || '',
            deviceid: r.deviceid || ''
        }));

        setLogs(transformed);
    }, [jwt, filter]);

    useEffect(() => {
        fetchGraphData();
    }, [fetchGraphData]);

    const formatChartData = useMemo(() => {
        return logs.map((log) => ({
            time: log.timestamp
                ? new Date(log.timestamp).toLocaleDateString([], {
                    month: 'short',
                    day: 'numeric',
                    hour: '2-digit',
                    minute: '2-digit',
                })
                : '',
            temperature: Number(log.temperature),
            humidity: Number(log.humidity),
            pressure: Number(log.pressure),
            airquality: Number(log.airquality),
        }));
    }, [logs]);

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
