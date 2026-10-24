import * as React from "react";

const InputOTP = ({ length = 4, onChange }) => {
  const [values, setValues] = React.useState(Array(length).fill(''));

  const handleChange = (index, val) => {
    const next = [...values];
    next[index] = val;
    setValues(next);
    onChange && onChange(next.join(''));
  };

  return (
    <div className="flex gap-2">
      {values.map((v, i) => (
        <input key={i} value={v} onChange={(e) => handleChange(i, e.target.value)} className="w-12 text-center" />
      ))}
    </div>
  );
};

export { InputOTP };
