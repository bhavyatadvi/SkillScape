import React, { useState } from 'react';
import { X } from 'lucide-react';
import { apiClient } from '../../services/api';

const CreateCircleModal = ({ isOpen, onClose, onCreated }) => {
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    careerDomain: '',
    customDomain: '',
    circleType: 'Domain Circle',
    tags: ''
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  if (!isOpen) return null;

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    const name = formData.name.trim();
    if (name.length < 3 || name.length > 100) {
      setError('Circle name must be between 3 and 100 characters.');
      return;
    }

    setLoading(true);
    try {
      // Use custom domain if "Other" is selected
      const finalDomain = formData.careerDomain === 'Other' ? formData.customDomain : formData.careerDomain;
      
      const payload = {
        name,
        description: formData.description,
        careerDomain: finalDomain,
        circleType: formData.circleType,
        tags: formData.tags
      };

      const res = await apiClient.post('/learningcircles/create', payload);
      setLoading(false);
      onCreated(res.data);
      onClose();
    } catch (err) {
      setError('Failed to create circle. Please try again.');
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50 backdrop-blur-sm">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-lg p-6 relative animate-in fade-in zoom-in duration-200">
        <button 
          onClick={onClose}
          className="absolute right-4 top-4 text-slate-400 hover:text-slate-600 transition-colors"
        >
          <X className="h-5 w-5" />
        </button>
        
        <h2 className="text-2xl font-bold text-slate-800 mb-6">Create Learning Circle</h2>
        
        {error && (
          <div className="bg-red-50 text-red-600 p-3 rounded-lg mb-4 text-sm">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Name</label>
            <input 
              required
              type="text"
              name="name"
              value={formData.name}
              onChange={handleChange}
              minLength={3}
              maxLength={100}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="e.g. React Mastery Group"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Description</label>
            <textarea 
              name="description"
              value={formData.description}
              onChange={handleChange}
              rows="3"
              className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="What is this circle about?"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Career Domain</label>
            <select
              required
              name="careerDomain"
              value={formData.careerDomain}
              onChange={handleChange}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="" disabled>Select a domain</option>
              <option value="Software Engineering">Software Engineering</option>
              <option value="Artificial Intelligence">Artificial Intelligence</option>
              <option value="Data Science">Data Science</option>
              <option value="Cyber Security">Cyber Security</option>
              <option value="UI/UX Design">UI/UX Design</option>
              <option value="Digital Marketing">Digital Marketing</option>
              <option value="Finance & Accounting">Finance & Accounting</option>
              <option value="Human Resources">Human Resources</option>
              <option value="Business & Management">Business & Management</option>
              <option value="Entrepreneurship">Entrepreneurship</option>
              <option value="Teaching & Education">Teaching & Education</option>
              <option value="Healthcare & Medicine">Healthcare & Medicine</option>
              <option value="Civil Engineering">Civil Engineering</option>
              <option value="Mechanical Engineering">Mechanical Engineering</option>
              <option value="Electrical Engineering">Electrical Engineering</option>
              <option value="Govt Job Preparation">Govt Job Preparation</option>
              <option value="Law">Law</option>
              <option value="Arts & Design">Arts & Design</option>
              <option value="Commerce">Commerce</option>
              <option value="Agriculture">Agriculture</option>
              <option value="Other">Other (Type manually)</option>
            </select>
          </div>

          {formData.careerDomain === 'Other' && (
            <div className="animate-in fade-in slide-in-from-top-2 duration-200">
              <label className="block text-sm font-medium text-slate-700 mb-1">Enter Custom Domain</label>
              <input 
                required
                type="text"
                name="customDomain"
                value={formData.customDomain}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="e.g. Architecture"
              />
            </div>
          )}

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Circle Type</label>
            <select
              name="circleType"
              value={formData.circleType}
              onChange={handleChange}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="Domain Circle">Domain Circle</option>
              <option value="Career Goal Circle">Career Goal Circle</option>
              <option value="Skill Development Circle">Skill Development Circle</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Tags (comma separated)</label>
            <input 
              type="text"
              name="tags"
              value={formData.tags}
              onChange={handleChange}
              className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="e.g. React, Nextjs, Beginners"
            />
          </div>

          <div className="flex justify-end gap-3 pt-4 border-t border-slate-100">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-slate-600 hover:bg-slate-100 rounded-lg transition-colors"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={loading}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50"
            >
              {loading ? 'Creating...' : 'Create Circle'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default CreateCircleModal;
