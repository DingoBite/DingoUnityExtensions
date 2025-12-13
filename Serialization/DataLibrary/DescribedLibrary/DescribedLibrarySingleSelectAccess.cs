using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bind;
using UnityEngine;

namespace DingoUnityExtensions.Serialization.DataLibrary.DescribedLibrary
{
    public enum LibrarySelectEvent
    {
        None,
        Opened,
        Closed,
    }
    
    public readonly struct LibrarySelectMessage
    {
        public readonly StructureRootDataDescriptor Descriptor;
        public readonly LibrarySelectEvent Event;
        
        public LibrarySelectMessage(StructureRootDataDescriptor descriptor, LibrarySelectEvent @event)
        {
            Descriptor = descriptor;
            Event = @event;
        }
    }

    public abstract class DescribedLibrarySingleSelectAccess<TAccessModel, TDescriptor> where TAccessModel : DescribedSelectedAccessModel<TDescriptor> where TDescriptor : StructureRootDataDescriptor
    {
        private readonly Bind<TAccessModel> _selectedAccess = new();
        private readonly Bind<LibrarySelectMessage> _librarySelectMessage = new();

        private readonly DescribedLibraryAccess<TDescriptor> _libraryAccess;
        
        public IReadonlyBind<IReadOnlyList<TDescriptor>> Descriptors => _libraryAccess?.Descriptors;
        public IReadonlyBind<LibrarySelectMessage> LibrarySelectMessage => _librarySelectMessage;
        public IReadonlyBind<TAccessModel> SelectedAccess => _selectedAccess;

        protected DescribedLibrarySingleSelectAccess(DescribedLibraryAccess<TDescriptor> libraryAccess)
        {
            _libraryAccess = libraryAccess;
        }

        public Task<TDescriptor> CreateBlankAsync(Action<TDescriptor> mutateAction = null)
        {
            return _libraryAccess.CreateAsync(id =>
            {
                var descriptor = Factory(id);
                mutateAction?.Invoke(descriptor);
                return descriptor;
            });
        }

        public TAccessModel OpenAccess(TDescriptor descriptor)
        {
            if (descriptor == null)
                return null;
            
            if (Descriptors.V.All(d => d.Id != descriptor.Id))
            {
                Debug.LogError($"[OPEN ERROR]. There is no access with id: {descriptor.Id}");
                return null;
            }

            if (_selectedAccess.V != null && _selectedAccess.V.Id == descriptor.Id)
            {
                Debug.Log($"[OPEN MESSAGE]. Access with id: {descriptor.Id} is already opened");
                return _selectedAccess.V;
            }

            var profileAccess = _libraryAccess.OpenAccess(descriptor);
            _selectedAccess.V = Factory(profileAccess, descriptor);
            _librarySelectMessage.V = new LibrarySelectMessage(descriptor, LibrarySelectEvent.Opened);
            return _selectedAccess.V;
        }

        public async Task SaveAsync(TDescriptor descriptor)
        {
            if (descriptor == null)
                return;
            
            await _libraryAccess.SaveAsync(descriptor);
        }

        public void CloseAccess(TDescriptor descriptor)
        {
            if (descriptor == null)
                return;
            
            if (_selectedAccess.V == null || _selectedAccess.V.Id != descriptor.Id)
                return;

            _libraryAccess.CloseAccess(descriptor);
            _selectedAccess.V = null;
            _librarySelectMessage.V = new LibrarySelectMessage(descriptor, LibrarySelectEvent.Closed);
        }

        public async Task DeleteStructureAsync(TDescriptor descriptor)
        {
            if (descriptor == null)
                return;
            
            await _libraryAccess.DeleteAsync(descriptor);
            CloseAccess(descriptor);
        }

        protected abstract TDescriptor Factory(string guid);
        protected abstract TAccessModel Factory(DataByPathLibrary dataByPathLibrary, TDescriptor descriptor);
    }
}