import {
    AreaChart,
    Area,
    CartesianGrid,
    XAxis,
    YAxis,
    Tooltip,
    ResponsiveContainer,
} from 'recharts';

type ChartCardProps = {
    title: string;
    dataKey: 'temperature' | 'humidity' | 'pressure' | 'airquality';
    color: string;
    data: {
        time: string;
        temperature: number;
        humidity: number;
        pressure: number;
        airquality: number;
    }[];
};

const CustomTooltip = ({ active, payload }: any) => {
    if (!active || !payload || !payload.length) return null;

    const { time, [payload[0].dataKey]: value } = payload[0].payload;

    return (
        <div className="bg-white dark:bg-gray-800 text-black dark:text-white p-2 border rounded shadow">
            <div className="text-sm font-semibold">Time: {time}</div>
            <div className="text-sm">
                {`${payload[0].name}: ${value}`}
            </div>
        </div>
    );
};

export default function ChartCard({ title, dataKey, color, data }: ChartCardProps) {
    const gradientId = `${dataKey}-gradient`;

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
                    <Tooltip content={<CustomTooltip />} />
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
    );
}
