#region Copyright
/*
 * Copyright 2019 Igor Kazantsev
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy
 * of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 */
#endregion

using System.Drawing;

namespace MySQL_Clear_standart
{
    internal class TextMeasurer
    {
        static Image _fakeImage;

        static public SizeF MeasureString(string text, Font font)
        {
            if (_fakeImage == null)
            {
                _fakeImage = new Bitmap(1, 1);
            }

            using (Graphics g = Graphics.FromImage(_fakeImage))
            {
                return g.MeasureString(text, font);
            }
        }
    }
}