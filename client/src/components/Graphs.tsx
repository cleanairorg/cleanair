import '../css/Graphs.css'
import { useAtom } from 'jotai'
import { DeviceLogsAtom } from '../atoms'
import { useMemo } from 'react'
import {
    AreaChart,
    Area,
    CartesianGrid,
    XAxis,
    YAxis,
    Tooltip,
    ResponsiveContainer,
} from 'recharts'
import useInitializeData from '../hooks/useInitializeData'

/*
TO DO:
Logging for backend
Making a menu for different graphs?
Pick time / a endpoint more that takes timespan as a param
Day and or hour
make endpoint just take the 10 newest measurements in db
take the whole timestamp and make a median of the day
*/

export default function Graphs() {
    useInitializeData() // fetches logs on load

    const [logs] = useAtom(DeviceLogsAtom)

    const formatChartData = useMemo(() => {
        return logs.map((log) => ({
            time: log.timestamp ? new Date(log.timestamp).toLocaleTimeString() : '',
            temperature: Number(log.temperature),
            humidity: Number(log.humidity),
            pressure: Number(log.pressure),
            airquality: Number(log.airquality),
        }))
    }, [logs])

    return (
        <div className="graphs-container px-4 py-6 grid grid-cols-1 md:grid-cols-2 gap-6">
            <ChartCard title="Temperature (°C)" color="#f87171" dataKey="temperature" data={formatChartData} />
            <ChartCard title="Humidity (%)" color="#60a5fa" dataKey="humidity" data={formatChartData} />
            <ChartCard title="Pressure (hPa)" color="#34d399" dataKey="pressure" data={formatChartData} />
            <ChartCard title="Air Quality (PPM)" color="#fbbf24" dataKey="airquality" data={formatChartData} />
        </div>
    )
}

function ChartCard({
                       title,
                       dataKey,
                       color,
                       data,
                   }: {
    title: string
    dataKey: 'temperature' | 'humidity' | 'pressure' | 'airquality'
    color: string
    data: {
        time: string
        temperature: number
        humidity: number
        pressure: number
        airquality: number
    }[]
}) {
    const gradientId = `${dataKey}-gradient`

    return (
        <div className="graph-card bg-white dark:bg-gray-900 p-4 rounded shadow min-w-[300px] flex-1 max-w-[48%]">
            <h3 className="text-lg font-semibold mb-2">{title}</h3>
            <ResponsiveContainer width="100%" height={250}>
                <AreaChart data={data} margin={{ top: 10, right: 20, left: 10, bottom: 5 }}>
                    <defs>
                        <linearGradient id={gradientId} x1="0" y1="0" x2="0" y2="1">
                            <stop offset="5%" stopColor={color} stopOpacity={0.4} />
                            <stop offset="95%" stopColor={color} stopOpacity={1} />
                        </linearGradient>
                    </defs>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="time" />
                    <YAxis />
                    <Tooltip formatter={(value: number) => `${value}`} />
                    <Area
                        type="monotone"
                        dataKey={dataKey}
                        stroke={color}
                        strokeWidth={2}
                        fill={`url(#${gradientId})`}
                        fillOpacity={1}
                        dot={false}
                    />
                </AreaChart>
            </ResponsiveContainer>
        </div>
    )
}
