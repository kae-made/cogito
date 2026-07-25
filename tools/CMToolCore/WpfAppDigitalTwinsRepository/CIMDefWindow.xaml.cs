using KAE.CMTools.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfAppDigitalTwinsRepository
{
    /// <summary>
    /// CIMDefWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class CIMDefWindow : Window
    {
        public CIMDefWindow()
        {
            InitializeComponent();
            DataContext = this;
            PropertiesOfSelectedClass = new ObservableCollection<PropertyItem>();
            this.Loaded += CIMDefWindow_Loaded;
        }

        private void CIMDefWindow_Loaded(object sender, RoutedEventArgs e)
        {
            tbDomainName.Text = $"Conceptual Domain : '{currentDomain.Name}'";
        }

        public ConceptualDomain ConceptualDomain
        {
            get => currentDomain;
            set
            {
                currentDomain = value;
                ConceptualClasses = new ObservableCollection<ConceptualClass>(currentDomain.ConceptualClasses.Values);
                Relationships = new ObservableCollection<Relationship>(currentDomain.Relationships.Values);
                SubClasses = new ObservableCollection<SubClassInfo>();
            }
        }

        protected ConceptualDomain currentDomain;
        public ObservableCollection<ConceptualClass> ConceptualClasses { get; set; }
        public ObservableCollection<Relationship> Relationships { get; set; }

        public ObservableCollection<PropertyItem> PropertiesOfSelectedClass { get; set; }

        public ObservableCollection<SubClassInfo> SubClasses { get; set; }

        public ConceptualClass SelectedClass
        {
            get => selectedClass;
            set
            {
                selectedClass= value;
                PropertiesOfSelectedClass.Clear();
                foreach (var propName in selectedClass.Properties.Keys)
                {
                    var prop = selectedClass.Properties[propName];
                    var propItem = new PropertyItem() { Name=propName };
                    propItem.Info = $"{prop.DataType.Name}";
                    if (prop.DataType.Name=="REFERENCE")
                    {
                        propItem.Info += $"(=>{prop.BaseDataType.Name})";
                    }
                    string idInfo = "";
                    foreach(var idLevel in selectedClass.Identities.Keys)
                    {
                        foreach (var idPropName in selectedClass.Identities[idLevel].Keys)
                        {
                            if (propName == idPropName)
                            {
                                if (string.IsNullOrEmpty(idInfo))
                                {
                                    idInfo = ", Identity[";
                                }
                                else
                                {
                                    idInfo += ",";
                                }
                                idInfo += $"{idLevel}";
                            }
                        }
                        if (!string.IsNullOrEmpty(idInfo))
                        {
                            idInfo+= "]";
                            propItem.Info += idInfo;
                        }
                    }
                    PropertiesOfSelectedClass.Add(propItem);
                }
            }
        }

        private ConceptualClass selectedClass;
        
        private void ClearRelationshipDetails()
        {
            tbRelKindBinary.FontWeight = FontWeights.Normal;
            tbRelEdgeClassRef.Text = "";
            tbRelEdgeRef.Text = "";

            tbRelEdgeClassPart.Text = "";
            tbRelEdgePart.Text = "";
            borderRelBinary.Background = Brushes.Transparent;

            tbRelKindAssoc.FontWeight = FontWeights.Normal;
            tbRelEdgeClassOne.Text = "";
            tbRelEdgeOne.Text = "";

            tbRelEdgeClassOther.Text = "";
            tbRelEdgeOther.Text = "";

            tbRelEdgeClassAssoc.Text = "";
            borderRelAssoc.Background = Brushes.Transparent;

            tbRelKindSuper.FontWeight = FontWeights.Normal;
            tbRelKindSub.FontWeight = FontWeights.Normal;
            tbRelEdgeClassSuper.Text = "";
            SubClasses.Clear();
            borderRelIsA.Background = Brushes.Transparent;
        }

        private Func<ConceptualClass, string> CClassTitleFormatter => c => $"'{c.Name}'" + "{" + $"{c.KeyLetter}, {c.Number}" + "}";
        public Relationship SelectedRelationship
        {
            get => selectedRelationship;
            set
            {
                selectedRelationship = value;
                if (selectedRelationship != null)
                {
                    ClearRelationshipDetails();
                    if (selectedRelationship is BinaryRelationship<ConceptualClass, ConceptualClass>)
                    {
                        var binRel = (BinaryRelationship<ConceptualClass, ConceptualClass>)selectedRelationship;
                        var refInst = binRel.ReferentEdge.EdgeInstance;
                        var parInst = binRel.ParticipantEdge.EdgeInstance;
                        tbRelEdgeClassRef.Text = CClassTitleFormatter(refInst);
                        using (var writer = new StringWriter())
                        {
                            writer.WriteLine($"  Multiplicity : {binRel.ReferentEdge.Multiplicity}");
                            writer.WriteLine($"  Phrase : '{binRel.ReferentEdge.Phrase}'");
                            tbRelEdgeRef.Text = writer.ToString();
                        }
                        tbRelEdgeClassPart.Text = CClassTitleFormatter(parInst);
                        using (var writer = new StringWriter())
                        {
                            writer.WriteLine($"  Multiplicity : {binRel.ParticipantEdge.Multiplicity}");
                            writer.WriteLine($"  Phrase : '{binRel.ParticipantEdge.Phrase}'");
                            tbRelEdgePart.Text = writer.ToString();
                        }

                        tbRelKindBinary.FontWeight = FontWeights.Bold;
                        borderRelBinary.Background = Brushes.AliceBlue;
                    }
                    else if (selectedRelationship is IsARelationship<ConceptualClass>)
                    {
                        var isARel = (IsARelationship<ConceptualClass>)selectedRelationship;
                        tbRelEdgeClassSuper.Text = CClassTitleFormatter(isARel.SuperEdge);

                        SubClasses.Clear();
                        foreach (var sub in isARel.SubEdges.Keys)
                        {
                            var subEdge = isARel.SubEdges[sub];
                            SubClasses.Add(new SubClassInfo() { Title = CClassTitleFormatter(subEdge.SubEdge), KeyLetter = subEdge.SubEdge.KeyLetter, ConceptualClass = subEdge.SubEdge });
                        }
                        tbRelKindSuper.FontWeight = FontWeights.Bold;
                        tbRelKindSub.FontWeight = FontWeights.Bold;
                        borderRelIsA.Background = Brushes.AliceBlue;
                    }
                    else if (selectedRelationship is AssociativeRelationship<ConceptualClass, ConceptualClass, ConceptualClass>)
                    {
                        var assocRel = (AssociativeRelationship<ConceptualClass, ConceptualClass, ConceptualClass>)selectedRelationship;
                        var oneInsst = assocRel.OneEdge.EdgeInstance;
                        var otherInst = assocRel.OtherEdge.EdgeInstance;
                        var assocInst = assocRel.AssocOnOneEdge.EdgeInstance;
                        tbRelEdgeClassOne.Text = CClassTitleFormatter(oneInsst);
                        using (var writer = new StringWriter())
                        {
                            writer.WriteLine($"  Multiplisity: {assocRel.OneSideRelationship.ReferentEdge.Multiplicity}");
                            writer.WriteLine($"  Phrase: {assocRel.OneSideRelationship.ReferentEdge.Phrase}");
                            tbRelEdgeOne.Text = writer.ToString();
                        }
                        tbRelEdgeClassOther.Text = CClassTitleFormatter(otherInst);
                        using (var writer = new StringWriter())
                        {
                            writer.WriteLine($"  Multiplisity: {assocRel.OtherSideRelationship.ReferentEdge.Multiplicity}");
                            writer.WriteLine($"  Phrase: {assocRel.OtherSideRelationship.ReferentEdge.Phrase}");
                            tbRelEdgeOther.Text = writer.ToString();
                        }
                        tbRelEdgeClassAssoc.Text = CClassTitleFormatter(assocInst);
                        tbRelKindAssoc.FontWeight = FontWeights.Bold;

                        borderRelAssoc.Background = Brushes.AliceBlue;
                    }
                }
            }
        }
        private Relationship selectedRelationship;

        private void tbRelEdgeClass_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tb = (TextBlock)sender;
            UpdateClassDescrip(tb.Text);
        }

        private void UpdateClassDescrip(string cclassTitle)
        {
            var pattern = @"\{([^,]+),";
            var m = Regex.Match(cclassTitle, pattern);
            if (m.Success)
            {
                string keyLett = m.Groups[1].Value;
                var targetClass = ConceptualClasses.FirstOrDefault(c => c.KeyLetter == keyLett);
                lbClasses.SelectedItem = targetClass;
            }
        }

        private void tbRelEdgeClass_TouchDown(object sender, TouchEventArgs e)
        {
            var tb = (TextBlock)sender;
            UpdateClassDescrip(tb.Text);
        }

        private void lbSubClasses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (SubClassInfo)lbSubClasses.SelectedItem;
            UpdateClassDescrip(selectedItem.Title);
        }
    }

    public class PropertyItem
    {
        public string Name { get; set; }
        public string Info { get; set; }
    }

    public class SubClassInfo
    {
        public string Title { get; set; }
        public string KeyLetter { get; set; }
        public ConceptualClass ConceptualClass { get; set; }
    }
}
