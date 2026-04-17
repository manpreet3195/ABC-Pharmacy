const { useEffect, useState } = React;

const emptyMedicineForm = {
  fullName: "",
  notes: "",
  expiryDate: "",
  quantity: "",
  price: "",
  brand: ""
};

const emptySaleForm = {
  medicineId: "",
  quantitySold: "1"
};

async function requestJson(url, options = {}) {
  const response = await fetch(url, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(options.headers || {})
    }
  });

  const payload = await response.json().catch(() => ({}));
  if (!response.ok) {
    const errorText = payload.error || (Array.isArray(payload.errors) ? payload.errors.join(" ") : "");
    throw new Error(errorText || `Request failed (${response.status})`);
  }

  return payload;
}

function daysUntil(dateValue) {
  const now = new Date();
  now.setHours(0, 0, 0, 0);
  const target = new Date(dateValue);
  target.setHours(0, 0, 0, 0);
  return Math.ceil((target - now) / (1000 * 60 * 60 * 24));
}

function App() {
  const [status, setStatus] = useState({ message: "Loading...", isError: false });
  const [medicines, setMedicines] = useState([]);
  const [sales, setSales] = useState([]);
  const [searchInput, setSearchInput] = useState("");
  const [searchText, setSearchText] = useState("");
  const [medicineForm, setMedicineForm] = useState(emptyMedicineForm);
  const [saleForm, setSaleForm] = useState(emptySaleForm);

  const setSuccess = message => setStatus({ message, isError: false });
  const setError = message => setStatus({ message, isError: true });

  const loadMedicines = async (searchValue = searchText) => {
    const query = searchValue ? `?search=${encodeURIComponent(searchValue)}` : "";
    const data = await requestJson(`/api/medicines${query}`);
    setMedicines(data);
    return data;
  };

  const loadSales = async () => {
    const data = await requestJson("/api/sales");
    setSales(data);
    return data;
  };

  useEffect(() => {
    const initialize = async () => {
      try {
        await Promise.all([loadMedicines(""), loadSales()]);
        setSuccess("Ready.");
      } catch (error) {
        setError(error.message);
      }
    };
    initialize();
  }, []);

  useEffect(() => {
    if (medicines.length === 0) {
      setSaleForm(prev => ({ ...prev, medicineId: "" }));
      return;
    }

    const medicineStillExists = medicines.some(m => m.id === saleForm.medicineId);
    if (!medicineStillExists) {
      setSaleForm(prev => ({ ...prev, medicineId: medicines[0].id }));
    }
  }, [medicines, saleForm.medicineId]);

  const handleMedicineChange = event => {
    const { name, value } = event.target;
    setMedicineForm(prev => ({ ...prev, [name]: value }));
  };

  const handleSaleChange = event => {
    const { name, value } = event.target;
    setSaleForm(prev => ({ ...prev, [name]: value }));
  };

  const handleMedicineSubmit = async event => {
    event.preventDefault();

    const payload = {
      fullName: medicineForm.fullName,
      notes: medicineForm.notes,
      expiryDate: medicineForm.expiryDate,
      quantity: Number.parseInt(medicineForm.quantity, 10),
      price: Number.parseFloat(medicineForm.price),
      brand: medicineForm.brand
    };

    try {
      await requestJson("/api/medicines", {
        method: "POST",
        body: JSON.stringify(payload)
      });

      setMedicineForm(emptyMedicineForm);
      setSuccess("Medicine added successfully.");
      await loadMedicines(searchText);
    } catch (error) {
      setError(error.message);
    }
  };

  const handleSaleSubmit = async event => {
    event.preventDefault();
    const payload = {
      medicineId: saleForm.medicineId,
      quantitySold: Number.parseInt(saleForm.quantitySold, 10)
    };

    try {
      await requestJson("/api/sales", {
        method: "POST",
        body: JSON.stringify(payload)
      });

      setSaleForm(prev => ({ ...prev, quantitySold: "1" }));
      setSuccess("Sale recorded successfully.");
      await Promise.all([loadMedicines(searchText), loadSales()]);
    } catch (error) {
      setError(error.message);
    }
  };

  const handleQuickSell = async medicineId => {
    try {
      await requestJson("/api/sales", {
        method: "POST",
        body: JSON.stringify({
          medicineId,
          quantitySold: 1
        })
      });

      setSuccess("Sale recorded.");
      await Promise.all([loadMedicines(searchText), loadSales()]);
    } catch (error) {
      setError(error.message);
    }
  };

  const handleSearchSubmit = async event => {
    event.preventDefault();
    const normalizedSearch = searchInput.trim();
    setSearchText(normalizedSearch);

    try {
      await loadMedicines(normalizedSearch);
    } catch (error) {
      setError(error.message);
    }
  };

  const handleClearSearch = async () => {
    setSearchInput("");
    setSearchText("");
    try {
      await loadMedicines("");
    } catch (error) {
      setError(error.message);
    }
  };

  return (
    <main className="container">
      <h1>ABC Pharmacy - Medicine Tracker</h1>
      <p className="subtitle">React.js single-page application for medicines and sales.</p>
      <div className={`status ${status.isError ? "error" : "ok"}`}>{status.message}</div>

      <section className="card">
        <h2>Add Medicine</h2>
        <form onSubmit={handleMedicineSubmit}>
          <div className="form-grid">
            <label>
              Full Name
              <input type="text" name="fullName" value={medicineForm.fullName} onChange={handleMedicineChange} required />
            </label>
            <label>
              Brand
              <input type="text" name="brand" value={medicineForm.brand} onChange={handleMedicineChange} required />
            </label>
            <label>
              Expiry Date
              <input type="date" name="expiryDate" value={medicineForm.expiryDate} onChange={handleMedicineChange} required />
            </label>
            <label>
              Quantity
              <input type="number" name="quantity" min="0" value={medicineForm.quantity} onChange={handleMedicineChange} required />
            </label>
            <label>
              Price
              <input type="number" name="price" min="0" step="0.01" value={medicineForm.price} onChange={handleMedicineChange} required />
            </label>
            <label className="full-width">
              Notes
              <textarea name="notes" rows="3" value={medicineForm.notes} onChange={handleMedicineChange} placeholder="Optional notes" />
            </label>
          </div>
          <button type="submit">Add Medicine</button>
        </form>
      </section>

      <section className="card">
        <h2>Medicine Inventory</h2>
        <form className="search-row" onSubmit={handleSearchSubmit}>
          <input
            type="text"
            value={searchInput}
            onChange={event => setSearchInput(event.target.value)}
            placeholder="Search by medicine name"
          />
          <button type="submit">Search</button>
          <button type="button" className="secondary" onClick={handleClearSearch}>Clear</button>
        </form>
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Full Name</th>
                <th>Expiry Date</th>
                <th>Quantity</th>
                <th>Price</th>
                <th>Brand</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              {medicines.length === 0 ? (
                <tr>
                  <td colSpan="6">No medicines found.</td>
                </tr>
              ) : (
                medicines.map(medicine => {
                  const expiryDays = daysUntil(medicine.expiryDate);
                  const rowClass = expiryDays < 30 ? "expiring-soon" : medicine.quantity < 10 ? "low-stock" : "";

                  return (
                    <tr key={medicine.id} className={rowClass}>
                      <td>{medicine.fullName}</td>
                      <td>{new Date(medicine.expiryDate).toLocaleDateString()}</td>
                      <td>{medicine.quantity}</td>
                      <td>{Number(medicine.price).toFixed(2)}</td>
                      <td>{medicine.brand}</td>
                      <td>
                        <button type="button" className="small" onClick={() => handleQuickSell(medicine.id)}>Sell 1</button>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
        <p className="legend">
          <span className="chip red">Red</span> Expiry date within 30 days
          <span className="chip yellow">Yellow</span> Stock below 10
        </p>
      </section>

      <section className="card">
        <h2>Record Sale</h2>
        <form onSubmit={handleSaleSubmit}>
          <div className="form-grid">
            <label>
              Medicine
              <select
                name="medicineId"
                value={saleForm.medicineId}
                onChange={handleSaleChange}
                disabled={medicines.length === 0}
                required
              >
                {medicines.length === 0 ? (
                  <option value="">No medicines available</option>
                ) : (
                  medicines.map(medicine => (
                    <option key={medicine.id} value={medicine.id}>
                      {medicine.fullName} ({medicine.quantity} in stock)
                    </option>
                  ))
                )}
              </select>
            </label>
            <label>
              Quantity Sold
              <input type="number" name="quantitySold" min="1" value={saleForm.quantitySold} onChange={handleSaleChange} required />
            </label>
          </div>
          <button type="submit" disabled={medicines.length === 0}>Save Sale</button>
        </form>
      </section>

      <section className="card">
        <h2>Sale Records</h2>
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Sold At (UTC)</th>
                <th>Medicine</th>
                <th>Quantity</th>
                <th>Unit Price</th>
                <th>Total</th>
              </tr>
            </thead>
            <tbody>
              {sales.length === 0 ? (
                <tr>
                  <td colSpan="5">No sale records found.</td>
                </tr>
              ) : (
                sales.map(sale => {
                  const total = Number(sale.unitPrice) * Number(sale.quantitySold);
                  return (
                    <tr key={sale.id}>
                      <td>{new Date(sale.soldAt).toISOString().replace("T", " ").slice(0, 19)}</td>
                      <td>{sale.medicineName}</td>
                      <td>{sale.quantitySold}</td>
                      <td>{Number(sale.unitPrice).toFixed(2)}</td>
                      <td>{total.toFixed(2)}</td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </section>
    </main>
  );
}

ReactDOM.createRoot(document.getElementById("root")).render(<App />);
