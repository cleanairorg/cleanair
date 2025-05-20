import { useEffect, useMemo } from 'react';
import { useAtom } from 'jotai';
import { DeviceLogsAtom, JwtAtom } from '../atoms';
import { weatherStationClient } from '../apiControllerClients';
import GraphFilter from './GraphFilter';
import type { Devicelog, TimeRangeDto } from '../generated-client';
import ChartCard from './ChartCard';

export default function Graphs() {
    const [logs, setLogs] = useAtom(DeviceLogsAtom);
    const [jwt] = useAtom(JwtAtom);

    const fetchGraphData = async (
        type: 'today' | 'weekly' | 'monthly',
        selectedMonth?: number,
        selectedYear?: number
    ) => {
        if (!jwt) return;

        let startDate: Date;
        let endDate: Date;

        if (type === 'today') {
            startDate = new Date();
            startDate.setHours(0, 0, 0, 0);
            endDate = new Date();
            endDate.setHours(23, 59, 59, 999);
        } else if (type === 'weekly') {
            endDate = new Date();
            startDate = new Date();
            startDate.setDate(endDate.getDate() - 6);
            startDate.setHours(0, 0, 0, 0);
            endDate.setHours(23, 59, 59, 999);
        } else if (type === 'monthly' && selectedMonth && selectedYear) {
            startDate = new Date(Date.UTC(selectedYear, selectedMonth - 1, 1));
            endDate = new Date(Date.UTC(selectedYear, selectedMonth, 0, 23, 59, 59));
        } else {
            return;
        }

        const dto = {
            startDate,
            endDate
        } satisfies TimeRangeDto;

        const result = await weatherStationClient.getDailyAverages(dto);

        const transformed: Devicelog[] = result.map(r => ({
            timestamp: r.date ? new Date(r.date) : undefined,
            temperature: r.avgTemperature,
            humidity: r.avgHumidity,
            pressure: r.avgPressure,
            airquality: r.avgAirQuality,
            id: crypto.randomUUID(),
            unit: '',
            deviceid: ''
        }));

        setLogs(transformed);
    };

    useEffect(() => {
        fetchGraphData('today');
    }, []);

    const formatChartData = useMemo(() => {
        return logs.map((log) => ({
            time: log.timestamp
                ? new Date(log.timestamp).toLocaleDateString([], {
                    month: 'short',
                    day: 'numeric',
                    hour: '2-digit',
                    minute: '2-digit'
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
            <GraphFilter onSelect={fetchGraphData} />
            <div className="graphs-container flex flex-wrap gap-6">
                <ChartCard title="Temperature (°C)" color="#f87171" dataKey="temperature" data={formatChartData}/>
                <ChartCard title="Humidity (%)" color="#60a5fa" dataKey="humidity" data={formatChartData}/>
                <ChartCard title="Pressure (hPa)" color="#34d399" dataKey="pressure" data={formatChartData}/>
                <ChartCard title="Air Quality (PPM)" color="#fbbf24" dataKey="airquality" data={formatChartData}/>
            </div>
        </div>
    );
}
