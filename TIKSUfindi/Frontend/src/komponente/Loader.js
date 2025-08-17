import React from 'react';

const Loader = () => {
  return (
<div className="fixed inset-0 flex items-center justify-center bg-gray-800 bg-opacity-50">
      <div className="flex items-center justify-center bg-orange-400 bg-opacity-60 p-8 rounded-lg">
        <div className="loader"></div>
      </div>
    </div>
  );
};

export default Loader;