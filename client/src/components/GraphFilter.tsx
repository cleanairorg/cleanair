import { useState } from 'react';

const months = [
    'Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
    'Jul', 'Aug', 'Sept', 'Oct', 'Nov', 'Dec'
];

type TimeOption = 'today' | 'weekly' | 'monthly';

type Props = {
    onSelect: (type: TimeOption, month?: number, year?: number) => void;
};

export default function GraphFilter({ onSelect }: Props) {
    const now = new Date();
    const [month, setMonth] = useState(now.getMonth() + 1);
    const [year, setYear] = useState(now.getFullYear());
    const [showSelector, setShowSelector] = useState(false);

    return (
        <div className="flex flex-col gap-3 mb-4">
            <div className="flex gap-2">
                <button className="btn btn-sm" onClick={() => onSelect('weekly')}>Last Week</button>
                <button className="btn btn-sm" onClick={() => onSelect('today')}>Today</button>
                <button className="btn btn-sm" onClick={() => setShowSelector(!showSelector)}>Monthly</button>
            </div>
            {showSelector && (
                <div className="mt-2 border rounded p-2 shadow bg-white dark:bg-gray-800 w-fit">
                    <div className="flex justify-between items-center mb-2">
                        <button onClick={() => setYear((y) => y - 1)} className="btn btn-xs">&lt;</button>
                        <span className="mx-4 font-semibold">{year}</span>
                        <button onClick={() => setYear((y) => y + 1)} className="btn btn-xs">&gt;</button>
                    </div>
                    <div className="grid grid-cols-4 gap-2">
                        {months.map((label, index) => (
                            <button
                                key={label}
                                className={`px-2 py-1 rounded text-sm ${index + 1 === month ? 'bg-blue-600 text-white' : 'hover:bg-blue-100'}`}
                                onClick={() => {
                                    setMonth(index + 1);
                                    onSelect('monthly', index + 1, year);
                                    setShowSelector(false);
                                }}
                            >
                                {label}
                            </button>
                        ))}
                    </div>
                </div>
            )}
        </div>
    );
}
